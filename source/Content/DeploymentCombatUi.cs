using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

namespace WeiDoctor.Content;

public static class DeploymentCombatUi
{
    private const string PanelName = "WeiDoctorDeploymentPanel";
    private const string LayerName = "WeiDoctorDeploymentLayer";
    private static CanvasLayer? CurrentLayer;
    private static DeploymentPanel? CurrentPanel;

    public static void Ensure(NCombatRoom? room = null)
    {
        room ??= NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
        {
            return;
        }

        if (room.Mode != CombatRoomMode.ActiveCombat)
        {
            Clear();
            return;
        }

        Viewport? root = room.GetTree()?.Root;
        if (root == null || !GodotObject.IsInstanceValid(root))
        {
            return;
        }

        Node? existingLayer = root.GetNodeOrNull(LayerName);
        if (existingLayer is CanvasLayer canvasLayer && GodotObject.IsInstanceValid(canvasLayer))
        {
            CurrentLayer = canvasLayer;
            Node? existingPanel = canvasLayer.GetNodeOrNull(PanelName);
            if (existingPanel is DeploymentPanel panelInLayer && GodotObject.IsInstanceValid(panelInLayer))
            {
                CurrentPanel = panelInLayer;
                return;
            }
        }

        if (CurrentLayer == null || !GodotObject.IsInstanceValid(CurrentLayer))
        {
            CurrentLayer = new CanvasLayer
            {
                Name = LayerName,
                Layer = 95,
                ProcessMode = Node.ProcessModeEnum.Always,
            };
            root.AddChild(CurrentLayer);
            Entry.Logger.Info("[DeploymentUI] Canvas layer created.");
        }

        Node? existing = CurrentLayer.GetNodeOrNull(PanelName);
        if (existing is DeploymentPanel deploymentPanel)
        {
            CurrentPanel = deploymentPanel;
            return;
        }

        DeploymentPanel panel = new()
        {
            Name = PanelName,
            ZIndex = 200,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        panel.BuildVisuals();
        CurrentLayer.AddChild(panel);
        CurrentPanel = panel;
        panel.Refresh(Array.Empty<DeploymentDisplayEntry>(), 3, "待部署");
        Entry.Logger.Info("[DeploymentUI] Panel created.");
    }

    public static void Refresh(IReadOnlyList<DeploymentDisplayEntry> deployments, int maxSlots)
    {
        Ensure();
        CurrentPanel?.Refresh(deployments, maxSlots, "待部署");
    }

    public static void Pulse()
    {
        CurrentPanel?.Pulse();
    }

    public static void Clear()
    {
        bool hadLayer = CurrentLayer != null && GodotObject.IsInstanceValid(CurrentLayer);
        if (hadLayer)
        {
            CurrentLayer!.QueueFree();
        }

        CurrentLayer = null;
        CurrentPanel = null;
        if (hadLayer)
        {
            Entry.Logger.Info("[DeploymentUI] Panel cleared.");
        }
    }
}

public readonly record struct DeploymentDisplayEntry(string Name, int RemainingTurns, string ArtRelativePath);

public sealed partial class DeploymentPanel : Control
{
    private readonly List<DeploymentSlot> _slots = new();
    private HBoxContainer? _slotRow;
    private IReadOnlyList<DeploymentDisplayEntry> _pendingDeployments = Array.Empty<DeploymentDisplayEntry>();
    private int _pendingMaxSlots = 3;
    private string _pendingEmptyText = "待部署";
    private bool _built;

    public override void _Ready()
    {
        BuildVisuals();
    }

    public void BuildVisuals()
    {
        if (_built)
        {
            SetPanelPosition();
            return;
        }

        _built = true;
        CustomMinimumSize = new Vector2(460f, 118f);
        SetPanelPosition();

        PanelContainer shell = new()
        {
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(460f, 118f),
        };
        shell.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.02f, 0.10f, 0.12f, 0.82f),
            BorderColor = new Color(0.23f, 0.82f, 0.74f, 0.88f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
        };
        shell.AddThemeStyleboxOverride("panel", style);

        VBoxContainer stack = new()
        {
            MouseFilter = MouseFilterEnum.Ignore,
        };
        stack.AddThemeConstantOverride("separation", 4);
        shell.AddChild(stack);

        Label title = new()
        {
            Text = "部署区",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        title.AddThemeColorOverride("font_color", new Color(0.80f, 1.00f, 0.94f));
        title.AddThemeFontSizeOverride("font_size", 18);
        stack.AddChild(title);

        _slotRow = new HBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        _slotRow.AddThemeConstantOverride("separation", 8);
        stack.AddChild(_slotRow);

        AddChild(shell);
        Refresh(_pendingDeployments, _pendingMaxSlots, _pendingEmptyText);
        Entry.Logger.Info($"[DeploymentUI] Panel built at {Position} size {Size}.");
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMSizeChanged)
        {
            SetPanelPosition();
        }
    }

    public override void _Process(double delta)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room) || room.Mode != CombatRoomMode.ActiveCombat)
        {
            DeploymentCombatUi.Clear();
        }
    }

    private void SetPanelPosition()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        Size = new Vector2(460f, 118f);
        Position = new Vector2(Mathf.Max(20f, (viewportSize.X - Size.X) * 0.5f), 142f);
        PivotOffset = Size * 0.5f;
    }

    public void Refresh(IReadOnlyList<DeploymentDisplayEntry> deployments, int maxSlots, string emptyText)
    {
        _pendingDeployments = deployments;
        _pendingMaxSlots = maxSlots;
        _pendingEmptyText = emptyText;

        if (_slotRow == null)
        {
            return;
        }

        while (_slots.Count < maxSlots)
        {
            DeploymentSlot slot = new();
            slot.BuildVisuals();
            _slots.Add(slot);
            _slotRow.AddChild(slot);
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < deployments.Count)
            {
                _slots[i].SetDeployment(deployments[i]);
            }
            else
            {
                _slots[i].SetEmpty(emptyText);
            }
        }
    }

    public void Pulse()
    {
        if (!IsInsideTree())
        {
            return;
        }

        Tween tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.One * 1.06f, 0.08f);
        tween.TweenProperty(this, "scale", Vector2.One, 0.16f).SetEase(Tween.EaseType.Out);
    }
}

public sealed partial class DeploymentSlot : PanelContainer
{
    private TextureRect? _portrait;
    private Label? _nameLabel;
    private Label? _turnLabel;
    private bool _built;

    public override void _Ready()
    {
        BuildVisuals();
    }

    public void BuildVisuals()
    {
        if (_built)
        {
            return;
        }

        _built = true;
        CustomMinimumSize = new Vector2(126f, 66f);
        MouseFilter = MouseFilterEnum.Ignore;

        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.05f, 0.18f, 0.20f, 0.88f),
            BorderColor = new Color(0.43f, 0.78f, 0.72f, 0.78f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5,
        };
        AddThemeStyleboxOverride("panel", style);

        HBoxContainer row = new()
        {
            MouseFilter = MouseFilterEnum.Ignore,
        };
        row.AddThemeConstantOverride("separation", 5);
        AddChild(row);

        _portrait = new TextureRect
        {
            CustomMinimumSize = new Vector2(50f, 50f),
            ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        row.AddChild(_portrait);

        VBoxContainer textStack = new()
        {
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        row.AddChild(textStack);

        _nameLabel = new Label
        {
            Text = "",
            ClipText = true,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _nameLabel.AddThemeFontSizeOverride("font_size", 15);
        _nameLabel.AddThemeColorOverride("font_color", new Color(0.94f, 0.99f, 0.95f));
        textStack.AddChild(_nameLabel);

        _turnLabel = new Label
        {
            Text = "",
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _turnLabel.AddThemeFontSizeOverride("font_size", 13);
        _turnLabel.AddThemeColorOverride("font_color", new Color(0.78f, 0.92f, 0.88f));
        textStack.AddChild(_turnLabel);
    }

    public void SetDeployment(DeploymentDisplayEntry entry)
    {
        if (_portrait != null)
        {
            _portrait.Texture = WeiDoctorAssets.LoadTexture(entry.ArtRelativePath);
            _portrait.Modulate = Colors.White;
        }

        if (_nameLabel != null)
        {
            _nameLabel.Text = entry.Name;
        }

        if (_turnLabel != null)
        {
            _turnLabel.Text = $"{entry.RemainingTurns} 回合";
        }
    }

    public void SetEmpty(string text)
    {
        if (_portrait != null)
        {
            _portrait.Texture = null;
            _portrait.Modulate = new Color(1f, 1f, 1f, 0.25f);
        }

        if (_nameLabel != null)
        {
            _nameLabel.Text = text;
        }

        if (_turnLabel != null)
        {
            _turnLabel.Text = "-";
        }
    }
}

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom._Ready))]
public static class CombatRoomDeploymentUiPatch
{
    public static void Postfix(NCombatRoom __instance)
    {
        DeploymentCombatUi.Ensure(__instance);
    }
}

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom._ExitTree))]
public static class CombatRoomDeploymentUiCleanupPatch
{
    public static void Prefix()
    {
        DeploymentCombatUi.Clear();
    }
}
