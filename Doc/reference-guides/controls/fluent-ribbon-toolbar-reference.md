---
title: Fluent.Ribbon RibbonToolBar Reference
description: Comprehensive reference for RibbonToolBar and its layout definition system
tags: [toolbar, controls, layout]
see_also:
  - fluent-ribbon-groupbox-reference.md
  - ../styling/fluent-ribbon-layout-reference-v2.md
---

# Fluent.Ribbon RibbonToolBar Reference

**Comprehensive reference for RibbonToolBar and its layout definition system.**

*Source: Fluent.Ribbon source code analysis - RibbonToolBar.cs, RibbonToolBarControlDefinition.cs, RibbonToolBarControlGroup.cs, RibbonToolBarControlGroupDefinition.cs, RibbonToolBarLayoutDefinition.cs, RibbonToolBarRow.cs, RibbonToolBar.xaml*

---

## Overview

`RibbonToolBar` is a specialized container for complex, multi-row layouts within a `RibbonGroupBox`. Unlike simple stacking of controls, RibbonToolBar provides a powerful layout definition system that allows you to arrange controls in rows and columns with precise control over sizing at different ribbon states.

### Key Capabilities

- **Row-based layouts** - Arrange controls in multiple horizontal rows
- **Automatic column wrapping** - Controls flow into new columns when rows fill up
- **Size-specific layouts** - Define different arrangements for Large, Middle, and Small states
- **Control grouping** - Visually group related controls together
- **Simplified mode support** - Separate layouts for simplified ribbon state
- **Automatic separators** - Vertical separators between columns

### When to Use RibbonToolBar

| Scenario | Use RibbonToolBar? |
|----------|-------------------|
| 6+ small controls in a group | Yes - row layout is more compact |
| Related controls that need visual grouping | Yes - use ControlGroupDefinition |
| Different layouts for Large vs Small ribbon states | Yes - multiple LayoutDefinitions |
| Simple 2-3 button arrangement | No - use direct RibbonGroupBox content |
| Single large button with icon | No - use Button directly in group |
| Controls that need individual sizing via SizeDefinition | Maybe - simpler with direct placement |

---

## Control Hierarchy

```
fluent:RibbonToolBar
├── Children (actual controls)
│   ├── Button x:Name="control1"
│   ├── Button x:Name="control2"
│   ├── ComboBox x:Name="control3"
│   └── ...
└── LayoutDefinitions (layout rules)
    ├── RibbonToolBarLayoutDefinition Size="Large"
    │   └── Rows
    │       ├── RibbonToolBarRow
    │       │   ├── RibbonToolBarControlDefinition Target="control1"
    │       │   └── RibbonToolBarControlDefinition Target="control2"
    │       └── RibbonToolBarRow
    │           └── RibbonToolBarControlGroupDefinition
    │               └── RibbonToolBarControlDefinition Target="control3"
    └── RibbonToolBarLayoutDefinition Size="Small"
        └── ...different arrangement...
```

---

## Core Classes

### fluent:RibbonToolBar

The main container that holds controls and layout definitions.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Children` | `ObservableCollection<FrameworkElement>` | (empty) | The actual control instances to be laid out |
| `LayoutDefinitions` | `ObservableCollection<RibbonToolBarLayoutDefinition>` | (empty) | Layout rules for different sizes |
| `SeparatorStyle` | `Style` | (theme default) | Style applied to column separators |
| `IsSimplified` | `bool` | false | (read-only) Whether simplified mode is active |
| `Size` | `RibbonControlSize` | Large | Current size state from parent group |

**Inherited from RibbonControl:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `object` | null | Not typically used for RibbonToolBar |
| `Icon` | `object` | null | Not typically used for RibbonToolBar |
| `CanAddToQuickAccessToolBar` | `bool` | false | Always false - QAT not supported |

#### Behavior Notes

- If `LayoutDefinitions` is empty, children are displayed using a simple wrap panel layout
- When multiple LayoutDefinitions exist, the one matching current `Size` is used
- If no exact size match exists, the closest available layout is selected
- Separators are automatically inserted between columns

---

### fluent:RibbonToolBarLayoutDefinition

Defines how controls are arranged for a specific ribbon size state.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Size` | `RibbonControlSize` | Large | Which ribbon state this layout applies to |
| `SizeDefinition` | `RibbonControlSizeDefinition` | null | Alternative size specification |
| `RowCount` | `int` | 3 | Number of rows per column before wrapping |
| `ForSimplified` | `bool` | false | Whether this layout is for simplified ribbon mode |
| `Rows` | `ObservableCollection<RibbonToolBarRow>` | (empty) | Row definitions containing controls |

#### Size Matching Logic

When the ribbon resizes, `RibbonToolBar` selects a LayoutDefinition using this priority:

1. Exact match for current `Size` and `ForSimplified` state
2. Closest available size (Large prefers Middle over Small, etc.)
3. First definition in the collection if no match found

```csharp
// From source: Size fallback order
RibbonControlSize.Large   -> Middle -> Small -> first
RibbonControlSize.Middle  -> Small -> Large -> first
RibbonControlSize.Small   -> Middle -> Large -> first
```

---

### fluent:RibbonToolBarRow

A single row within a layout definition.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Children` | `ObservableCollection<DependencyObject>` | (empty) | Control or group definitions for this row |

#### Valid Children

- `RibbonToolBarControlDefinition` - Reference to a single control
- `RibbonToolBarControlGroupDefinition` - Reference to grouped controls

---

### fluent:RibbonToolBarControlDefinition

A reference to a control by name, with optional size override.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Target` | `string` | null | `x:Name` of the control in Children collection |
| `Size` | `RibbonControlSize` | Small | Size to apply to the control in this layout |
| `SizeDefinition` | `RibbonControlSizeDefinition` | null | Alternative size specification |
| `Width` | `double` | NaN | Explicit width override for the control |

---

### fluent:RibbonToolBarControlGroupDefinition

Groups multiple controls together visually (with shared background/border in some themes).

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Children` | `ObservableCollection<RibbonToolBarControlDefinition>` | (empty) | Control definitions within this group |

#### Events

| Event | Description |
|-------|-------------|
| `ChildrenChanged` | Fired when the Children collection changes |

---

### fluent:RibbonToolBarControlGroup

The visual container created at runtime to display grouped controls. You do not create these directly - they are generated from `RibbonToolBarControlGroupDefinition`.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | ItemCollection | (inherited) | Contains the actual control instances |
| `IsFirstInRow` | `bool` | true | Whether this group is first in its row |
| `IsLastInRow` | `bool` | true | Whether this group is last in its row |

---

## Layout Mechanics

### Row and Column Flow

Controls flow through the layout as follows:

1. Rows fill left-to-right with controls from each `RibbonToolBarRow`
2. After `RowCount` rows, a new column starts with a vertical separator
3. Row height is determined by the first control measured
4. Whitespace is distributed evenly between rows

```
Column 1          |  Column 2
------------------+------------------
Row 1: [Ctrl1] [Ctrl2]  |  Row 1: [Ctrl5]
Row 2: [Ctrl3]         |  Row 2: [Ctrl6]
Row 3: [Ctrl4]         |  Row 3: [Ctrl7] [Ctrl8]
```

### Wrap Panel Fallback

When no `LayoutDefinitions` are defined, `RibbonToolBar` uses a simple wrap panel algorithm:
- Controls stack vertically until available height is exhausted
- Then a new column starts
- No separators are added
- Each control gets its natural size

---

## Basic Examples

### Simple Three-Row Layout

```xml
<fluent:RibbonGroupBox Header="Paragraph">
    <fluent:RibbonToolBar>
        <!-- Actual controls -->
        <fluent:RibbonToolBar.Children>
            <fluent:Button x:Name="AlignLeft" Header="Left" Icon="align-left.png" Size="Small" />
            <fluent:Button x:Name="AlignCenter" Header="Center" Icon="align-center.png" Size="Small" />
            <fluent:Button x:Name="AlignRight" Header="Right" Icon="align-right.png" Size="Small" />
            <fluent:Button x:Name="BulletList" Header="Bullets" Icon="bullets.png" Size="Small" />
            <fluent:Button x:Name="NumberList" Header="Numbers" Icon="numbers.png" Size="Small" />
            <fluent:Button x:Name="Indent" Header="Indent" Icon="indent.png" Size="Small" />
        </fluent:RibbonToolBar.Children>

        <!-- Layout definition -->
        <fluent:RibbonToolBar.LayoutDefinitions>
            <fluent:RibbonToolBarLayoutDefinition RowCount="3">
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="AlignLeft" />
                    <fluent:RibbonToolBarControlDefinition Target="AlignCenter" />
                    <fluent:RibbonToolBarControlDefinition Target="AlignRight" />
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="BulletList" />
                    <fluent:RibbonToolBarControlDefinition Target="NumberList" />
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="Indent" />
                </fluent:RibbonToolBarRow>
            </fluent:RibbonToolBarLayoutDefinition>
        </fluent:RibbonToolBar.LayoutDefinitions>
    </fluent:RibbonToolBar>
</fluent:RibbonGroupBox>
```

### Using Control Groups

```xml
<fluent:RibbonToolBar>
    <fluent:RibbonToolBar.Children>
        <fluent:Button x:Name="Bold" Header="B" Size="Small" />
        <fluent:Button x:Name="Italic" Header="I" Size="Small" />
        <fluent:Button x:Name="Underline" Header="U" Size="Small" />
        <fluent:Button x:Name="FontSize" Header="Size" Size="Small" />
        <fluent:ComboBox x:Name="FontFamily" Width="100" />
    </fluent:RibbonToolBar.Children>

    <fluent:RibbonToolBar.LayoutDefinitions>
        <fluent:RibbonToolBarLayoutDefinition RowCount="2">
            <fluent:RibbonToolBarRow>
                <!-- Group formatting buttons together -->
                <fluent:RibbonToolBarControlGroupDefinition>
                    <fluent:RibbonToolBarControlDefinition Target="Bold" />
                    <fluent:RibbonToolBarControlDefinition Target="Italic" />
                    <fluent:RibbonToolBarControlDefinition Target="Underline" />
                </fluent:RibbonToolBarControlGroupDefinition>
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="FontFamily" />
                <fluent:RibbonToolBarControlDefinition Target="FontSize" />
            </fluent:RibbonToolBarRow>
        </fluent:RibbonToolBarLayoutDefinition>
    </fluent:RibbonToolBar.LayoutDefinitions>
</fluent:RibbonToolBar>
```

---

## Size-Specific Layouts

### Multiple Layout Definitions

Define different arrangements for each ribbon state:

```xml
<fluent:RibbonToolBar>
    <fluent:RibbonToolBar.Children>
        <fluent:Button x:Name="Cut" Header="Cut" Icon="cut.png" />
        <fluent:Button x:Name="Copy" Header="Copy" Icon="copy.png" />
        <fluent:Button x:Name="Paste" Header="Paste" Icon="paste.png" />
        <fluent:Button x:Name="Undo" Header="Undo" Icon="undo.png" />
        <fluent:Button x:Name="Redo" Header="Redo" Icon="redo.png" />
        <fluent:Button x:Name="Format" Header="Format" Icon="format.png" />
    </fluent:RibbonToolBar.Children>

    <fluent:RibbonToolBar.LayoutDefinitions>
        <!-- Large: All in one column, 3 rows -->
        <fluent:RibbonToolBarLayoutDefinition Size="Large" RowCount="3">
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Cut" />
                <fluent:RibbonToolBarControlDefinition Target="Copy" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Paste" />
                <fluent:RibbonToolBarControlDefinition Target="Format" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Undo" />
                <fluent:RibbonToolBarControlDefinition Target="Redo" />
            </fluent:RibbonToolBarRow>
        </fluent:RibbonToolBarLayoutDefinition>

        <!-- Middle: 2 columns -->
        <fluent:RibbonToolBarLayoutDefinition Size="Middle" RowCount="2">
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Cut" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Copy" />
            </fluent:RibbonToolBarRow>
            <!-- New column starts here (after RowCount=2) -->
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Paste" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Undo" />
            </fluent:RibbonToolBarRow>
            <!-- Third column -->
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Redo" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Format" />
            </fluent:RibbonToolBarRow>
        </fluent:RibbonToolBarLayoutDefinition>

        <!-- Small: Single row of icons -->
        <fluent:RibbonToolBarLayoutDefinition Size="Small" RowCount="1">
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Cut" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Copy" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Paste" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Undo" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Redo" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Format" Size="Small" />
            </fluent:RibbonToolBarRow>
        </fluent:RibbonToolBarLayoutDefinition>
    </fluent:RibbonToolBar.LayoutDefinitions>
</fluent:RibbonToolBar>
```

### Simplified Mode Layout

```xml
<fluent:RibbonToolBar>
    <fluent:RibbonToolBar.Children>
        <fluent:Button x:Name="Tool1" Header="Tool 1" />
        <fluent:Button x:Name="Tool2" Header="Tool 2" />
        <fluent:Button x:Name="Tool3" Header="Tool 3" />
    </fluent:RibbonToolBar.Children>

    <fluent:RibbonToolBar.LayoutDefinitions>
        <!-- Normal ribbon mode -->
        <fluent:RibbonToolBarLayoutDefinition ForSimplified="False" RowCount="3">
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Tool1" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Tool2" />
            </fluent:RibbonToolBarRow>
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Tool3" />
            </fluent:RibbonToolBarRow>
        </fluent:RibbonToolBarLayoutDefinition>

        <!-- Simplified ribbon mode - single row -->
        <fluent:RibbonToolBarLayoutDefinition ForSimplified="True" RowCount="1">
            <fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarControlDefinition Target="Tool1" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Tool2" Size="Small" />
                <fluent:RibbonToolBarControlDefinition Target="Tool3" Size="Small" />
            </fluent:RibbonToolBarRow>
        </fluent:RibbonToolBarLayoutDefinition>
    </fluent:RibbonToolBar.LayoutDefinitions>
</fluent:RibbonToolBar>
```

---

## Control Width Override

Use the `Width` property on `RibbonToolBarControlDefinition` to set explicit widths:

```xml
<fluent:RibbonToolBarRow>
    <!-- ComboBox with fixed width -->
    <fluent:RibbonToolBarControlDefinition Target="FontCombo" Width="120" />

    <!-- Spinner with fixed width -->
    <fluent:RibbonToolBarControlDefinition Target="FontSizeSpinner" Width="60" />
</fluent:RibbonToolBarRow>
```

---

## Separator Styling

The default separator style is defined in the theme. To customize:

```xml
<fluent:RibbonToolBar>
    <fluent:RibbonToolBar.SeparatorStyle>
        <Style TargetType="{x:Type Separator}"
               BasedOn="{StaticResource Fluent.Ribbon.Styles.RibbonSeparator}">
            <Setter Property="Margin" Value="8,4,8,4" />
            <Setter Property="Background" Value="Gray" />
        </Style>
    </fluent:RibbonToolBar.SeparatorStyle>

    <!-- ... -->
</fluent:RibbonToolBar>
```

---

## RibbonToolBar vs Regular Group Content

### Use RibbonToolBar When:

1. **Many small controls** - 6+ buttons that would waste space as large icons
2. **Complex row arrangements** - Controls need specific row/column placement
3. **Size-adaptive layouts** - Different arrangements for Large/Middle/Small
4. **Visual grouping** - Related controls should appear as a unit
5. **Multi-column layouts** - Controls should wrap into columns

### Use Direct RibbonGroupBox Content When:

1. **Few controls** - 1-3 buttons with clear visual hierarchy
2. **Simple stacking** - Default vertical or horizontal arrangement works
3. **Individual sizing** - Each control needs its own SizeDefinition
4. **Mixed large/small** - One large button + several small buttons

### Comparison Example

```xml
<!-- Simple case: Direct content is cleaner -->
<fluent:RibbonGroupBox Header="Clipboard">
    <fluent:Button Header="Paste" LargeIcon="paste.png" SizeDefinition="Large,Middle,Small" />
    <fluent:Button Header="Cut" Icon="cut.png" SizeDefinition="Middle,Small,Small" />
    <fluent:Button Header="Copy" Icon="copy.png" SizeDefinition="Middle,Small,Small" />
</fluent:RibbonGroupBox>

<!-- Complex case: RibbonToolBar provides better control -->
<fluent:RibbonGroupBox Header="Font">
    <fluent:RibbonToolBar>
        <fluent:RibbonToolBar.Children>
            <fluent:ComboBox x:Name="FontFamily" Width="120" />
            <fluent:Spinner x:Name="FontSize" Width="50" />
            <fluent:Button x:Name="Bold" Header="B" Size="Small" />
            <fluent:Button x:Name="Italic" Header="I" Size="Small" />
            <fluent:Button x:Name="Underline" Header="U" Size="Small" />
            <fluent:Button x:Name="Strikethrough" Header="S" Size="Small" />
            <fluent:DropDownButton x:Name="FontColor" Header="A" Size="Small" />
            <fluent:DropDownButton x:Name="Highlight" Header="H" Size="Small" />
        </fluent:RibbonToolBar.Children>

        <fluent:RibbonToolBar.LayoutDefinitions>
            <fluent:RibbonToolBarLayoutDefinition RowCount="2">
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="FontFamily" Width="120" />
                    <fluent:RibbonToolBarControlDefinition Target="FontSize" Width="50" />
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlGroupDefinition>
                        <fluent:RibbonToolBarControlDefinition Target="Bold" />
                        <fluent:RibbonToolBarControlDefinition Target="Italic" />
                        <fluent:RibbonToolBarControlDefinition Target="Underline" />
                        <fluent:RibbonToolBarControlDefinition Target="Strikethrough" />
                    </fluent:RibbonToolBarControlGroupDefinition>
                    <fluent:RibbonToolBarControlGroupDefinition>
                        <fluent:RibbonToolBarControlDefinition Target="FontColor" />
                        <fluent:RibbonToolBarControlDefinition Target="Highlight" />
                    </fluent:RibbonToolBarControlGroupDefinition>
                </fluent:RibbonToolBarRow>
            </fluent:RibbonToolBarLayoutDefinition>
        </fluent:RibbonToolBar.LayoutDefinitions>
    </fluent:RibbonToolBar>
</fluent:RibbonGroupBox>
```

---

## DO NOT DO

### Common Mistakes to Avoid

1. **DO NOT put controls directly in Rows**
   ```xml
   <!-- WRONG - controls go in Children, not Rows -->
   <fluent:RibbonToolBarRow>
       <fluent:Button Header="Wrong" />
   </fluent:RibbonToolBarRow>

   <!-- CORRECT - use ControlDefinition with Target -->
   <fluent:RibbonToolBarRow>
       <fluent:RibbonToolBarControlDefinition Target="MyButton" />
   </fluent:RibbonToolBarRow>
   ```

2. **DO NOT forget x:Name on controls**
   ```xml
   <!-- WRONG - no name means Target can't find it -->
   <fluent:RibbonToolBar.Children>
       <fluent:Button Header="Save" />
   </fluent:RibbonToolBar.Children>

   <!-- CORRECT -->
   <fluent:RibbonToolBar.Children>
       <fluent:Button x:Name="SaveButton" Header="Save" />
   </fluent:RibbonToolBar.Children>
   ```

3. **DO NOT mismatch Target names**
   - Target is case-sensitive
   - Control must exist in Children collection
   - Missing control is silently ignored

4. **DO NOT use RibbonToolBar for simple layouts**
   - Adds complexity without benefit
   - Direct placement in RibbonGroupBox is cleaner for 1-3 controls

5. **DO NOT expect QAT support**
   - `CanAddToQuickAccessToolBar` is explicitly disabled
   - Individual controls inside may support QAT, but the toolbar itself does not

6. **DO NOT put LayoutDefinitions without any Rows**
   ```xml
   <!-- WRONG - empty layout definition does nothing useful -->
   <fluent:RibbonToolBarLayoutDefinition Size="Large" />
   ```

7. **DO NOT forget to set RowCount appropriately**
   - Default is 3, which may not match your design
   - RowCount determines when columns wrap

8. **DO NOT reference the same control in multiple ControlDefinitions within the same layout**
   - A control can only appear once per layout
   - Use different layouts for different arrangements

9. **DO NOT mix ForSimplified values in layout selection expectations**
   - `ForSimplified="True"` layouts ONLY apply when ribbon is in simplified mode
   - `ForSimplified="False"` (default) layouts ONLY apply in normal mode
   - You need both if supporting simplified ribbon

10. **DO NOT set Size on ControlDefinition without understanding propagation**
    ```xml
    <!-- The Size here overrides the control's own Size property -->
    <fluent:RibbonToolBarControlDefinition Target="MyButton" Size="Small" />
    ```

---

## Complete Example

A comprehensive example showing all features:

```xml
<fluent:RibbonGroupBox Header="Drawing Tools">
    <fluent:RibbonToolBar>
        <!-- Define all controls with names -->
        <fluent:RibbonToolBar.Children>
            <!-- Shape tools -->
            <fluent:Button x:Name="Rectangle" Header="Rectangle" Icon="rect.png" Size="Small" />
            <fluent:Button x:Name="Ellipse" Header="Ellipse" Icon="ellipse.png" Size="Small" />
            <fluent:Button x:Name="Line" Header="Line" Icon="line.png" Size="Small" />
            <fluent:Button x:Name="Polygon" Header="Polygon" Icon="polygon.png" Size="Small" />

            <!-- Transform tools -->
            <fluent:Button x:Name="Rotate" Header="Rotate" Icon="rotate.png" Size="Small" />
            <fluent:Button x:Name="Scale" Header="Scale" Icon="scale.png" Size="Small" />
            <fluent:Button x:Name="Flip" Header="Flip" Icon="flip.png" Size="Small" />

            <!-- Color controls -->
            <fluent:DropDownButton x:Name="FillColor" Header="Fill" Icon="fill.png" Size="Small" />
            <fluent:DropDownButton x:Name="StrokeColor" Header="Stroke" Icon="stroke.png" Size="Small" />
            <fluent:Spinner x:Name="StrokeWidth" Header="Width" Value="1" Minimum="1" Maximum="20" />
        </fluent:RibbonToolBar.Children>

        <!-- Custom separator style -->
        <fluent:RibbonToolBar.SeparatorStyle>
            <Style TargetType="{x:Type Separator}"
                   BasedOn="{StaticResource Fluent.Ribbon.Styles.RibbonSeparator}">
                <Setter Property="Margin" Value="4" />
            </Style>
        </fluent:RibbonToolBar.SeparatorStyle>

        <fluent:RibbonToolBar.LayoutDefinitions>
            <!-- Large layout: 3 rows, shapes + transforms + colors -->
            <fluent:RibbonToolBarLayoutDefinition Size="Large" RowCount="3">
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlGroupDefinition>
                        <fluent:RibbonToolBarControlDefinition Target="Rectangle" />
                        <fluent:RibbonToolBarControlDefinition Target="Ellipse" />
                        <fluent:RibbonToolBarControlDefinition Target="Line" />
                        <fluent:RibbonToolBarControlDefinition Target="Polygon" />
                    </fluent:RibbonToolBarControlGroupDefinition>
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlGroupDefinition>
                        <fluent:RibbonToolBarControlDefinition Target="Rotate" />
                        <fluent:RibbonToolBarControlDefinition Target="Scale" />
                        <fluent:RibbonToolBarControlDefinition Target="Flip" />
                    </fluent:RibbonToolBarControlGroupDefinition>
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="FillColor" />
                    <fluent:RibbonToolBarControlDefinition Target="StrokeColor" />
                    <fluent:RibbonToolBarControlDefinition Target="StrokeWidth" Width="70" />
                </fluent:RibbonToolBarRow>
            </fluent:RibbonToolBarLayoutDefinition>

            <!-- Middle layout: 2 rows, more columns -->
            <fluent:RibbonToolBarLayoutDefinition Size="Middle" RowCount="2">
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="Rectangle" />
                    <fluent:RibbonToolBarControlDefinition Target="Ellipse" />
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="Line" />
                    <fluent:RibbonToolBarControlDefinition Target="Polygon" />
                </fluent:RibbonToolBarRow>
                <!-- Column 2 -->
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="Rotate" />
                    <fluent:RibbonToolBarControlDefinition Target="Flip" />
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="Scale" />
                </fluent:RibbonToolBarRow>
                <!-- Column 3 -->
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="FillColor" />
                </fluent:RibbonToolBarRow>
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="StrokeColor" />
                </fluent:RibbonToolBarRow>
            </fluent:RibbonToolBarLayoutDefinition>

            <!-- Small layout: Minimal controls in single row -->
            <fluent:RibbonToolBarLayoutDefinition Size="Small" RowCount="1">
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlGroupDefinition>
                        <fluent:RibbonToolBarControlDefinition Target="Rectangle" Size="Small" />
                        <fluent:RibbonToolBarControlDefinition Target="Ellipse" Size="Small" />
                        <fluent:RibbonToolBarControlDefinition Target="Line" Size="Small" />
                    </fluent:RibbonToolBarControlGroupDefinition>
                    <fluent:RibbonToolBarControlGroupDefinition>
                        <fluent:RibbonToolBarControlDefinition Target="FillColor" Size="Small" />
                        <fluent:RibbonToolBarControlDefinition Target="StrokeColor" Size="Small" />
                    </fluent:RibbonToolBarControlGroupDefinition>
                </fluent:RibbonToolBarRow>
            </fluent:RibbonToolBarLayoutDefinition>

            <!-- Simplified mode layout -->
            <fluent:RibbonToolBarLayoutDefinition ForSimplified="True" RowCount="1">
                <fluent:RibbonToolBarRow>
                    <fluent:RibbonToolBarControlDefinition Target="Rectangle" Size="Small" />
                    <fluent:RibbonToolBarControlDefinition Target="Ellipse" Size="Small" />
                    <fluent:RibbonToolBarControlDefinition Target="FillColor" Size="Small" />
                    <fluent:RibbonToolBarControlDefinition Target="StrokeColor" Size="Small" />
                </fluent:RibbonToolBarRow>
            </fluent:RibbonToolBarLayoutDefinition>
        </fluent:RibbonToolBar.LayoutDefinitions>
    </fluent:RibbonToolBar>
</fluent:RibbonGroupBox>
```

---

## Source File References

| File | Purpose |
|------|---------|
| `Controls/RibbonToolBar.cs` | Main container with layout logic |
| `Controls/RibbonToolBarControlDefinition.cs` | Single control reference |
| `Controls/RibbonToolBarControlGroup.cs` | Visual group container (runtime) |
| `Controls/RibbonToolBarControlGroupDefinition.cs` | Group definition |
| `Controls/RibbonToolBarLayoutDefinition.cs` | Size-specific layout configuration |
| `Controls/RibbonToolBarRow.cs` | Row container for definitions |
| `Themes/Controls/RibbonToolBar.xaml` | Default styles and templates |

---

## Related Resources

- `fluent-ribbon-controls-reference-v2.md` - Full control reference
- `fluent-ribbon-simplified-ribbon-reference.md` - Simplified mode details
- `fluent-ribbon-layout-reference-v2.md` - General ribbon sizing and layout
