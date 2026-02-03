# Fluent.Ribbon Reference Documentation

Comprehensive reference guides for the [Fluent.Ribbon](https://github.com/fluentribbon/Fluent.Ribbon) WPF library.

These guides focus on **practical usage patterns** and the **public API** - no visual tree hacking or internal implementation details.

---

## Quick Start

| If you want to... | Read this |
|-------------------|-----------|
| See all available controls | [getting-started/controls-reference-v2](getting-started/fluent-ribbon-controls-reference-v2.md) |
| Set up theming | [styling/controlzex-theming-reference-v2](styling/controlzex-theming-reference-v2.md) |
| Use MVVM patterns | [getting-started/mvvm-patterns](getting-started/fluent-ribbon-mvvm-patterns.md) |
| Fix a common issue | [reference/troubleshooting](reference/fluent-ribbon-troubleshooting.md) |
| Migrate from Microsoft Ribbon | [getting-started/migration-guide](getting-started/fluent-ribbon-migration-guide.md) |
| Look up a term | [glossary](glossary.md) |

---

## Folder Structure

```
fluent-ribbon-ref/
├── getting-started/    # Foundational guides (4 files)
├── controls/           # Specific control references (14 files)
├── ux/                 # User experience features (6 files)
├── styling/            # Theming and appearance (6 files)
├── advanced/           # Deep technical topics (4 files)
├── reference/          # Troubleshooting and meta (3 files)
├── archive/v1/         # Superseded original versions (11 files)
├── glossary.md         # Term definitions
├── LICENSE             # MIT License
└── PLAN.md             # Project history
```

---

## Guide Index

### Getting Started
| Guide | Description |
|-------|-------------|
| [controls-reference-v2](getting-started/fluent-ribbon-controls-reference-v2.md) | Overview of all ~30 ribbon controls |
| [common-tasks-v2](getting-started/fluent-ribbon-common-tasks-v2.md) | Basic tasks: themes, icons, commands |
| [mvvm-patterns](getting-started/fluent-ribbon-mvvm-patterns.md) | Commands, bindings, ViewModels |
| [migration-guide](getting-started/fluent-ribbon-migration-guide.md) | Microsoft Ribbon to Fluent.Ribbon |

### Controls
| Guide | Description |
|-------|-------------|
| [groupbox-reference](controls/fluent-ribbon-groupbox-reference.md) | RibbonGroupBox deep dive |
| [button-controls-reference](controls/fluent-ribbon-button-controls-reference.md) | Button, ToggleButton, SplitButton, DropDownButton |
| [input-controls-reference](controls/fluent-ribbon-input-controls-reference.md) | Spinner, TextBox, ComboBox |
| [menu-controls-reference](controls/fluent-ribbon-menu-controls-reference.md) | MenuItem, RibbonMenu, context menus |
| [galleries-reference](controls/fluent-ribbon-galleries-reference.md) | InRibbonGallery, Gallery, GalleryItem |
| [qat-reference-v2](controls/fluent-ribbon-qat-reference-v2.md) | Quick Access Toolbar |
| [backstage-reference](controls/fluent-ribbon-backstage-reference.md) | Backstage (File menu) |
| [applicationmenu-reference](controls/fluent-ribbon-applicationmenu-reference.md) | Application menu (Office 2007 style) |
| [contextual-tabs-reference](controls/fluent-ribbon-contextual-tabs-reference.md) | Context-sensitive tabs |
| [titlebar-reference](controls/fluent-ribbon-titlebar-reference.md) | RibbonTitleBar |
| [statusbar-reference](controls/fluent-ribbon-statusbar-reference.md) | StatusBar |
| [startscreen-reference](controls/fluent-ribbon-startscreen-reference.md) | StartScreen overlay |
| [toolbar-reference](controls/fluent-ribbon-toolbar-reference.md) | RibbonToolBar |
| [misc-controls-reference](controls/fluent-ribbon-misc-controls-reference.md) | TwoLineLabel, Separator, interfaces |

### User Experience
| Guide | Description |
|-------|-------------|
| [keytips-reference](ux/fluent-ribbon-keytips-reference.md) | Keyboard navigation, KeyTips |
| [screentip-reference](ux/fluent-ribbon-screentip-reference.md) | Enhanced tooltips |
| [accessibility-reference](ux/fluent-ribbon-accessibility-reference.md) | AutomationPeers, screen readers |
| [simplified-ribbon-reference](ux/fluent-ribbon-simplified-ribbon-reference.md) | Simplified/collapsed ribbon mode |
| [state-persistence-reference](ux/fluent-ribbon-state-persistence-reference.md) | Save/restore ribbon state |
| [localization-reference](ux/fluent-ribbon-localization-reference.md) | 37 languages, translations |

### Styling & Theming
| Guide | Description |
|-------|-------------|
| [brushes-reference-v2](styling/fluent-ribbon-brushes-reference-v2.md) | ~145 brush resource keys |
| [controlzex-theming-reference-v2](styling/controlzex-theming-reference-v2.md) | ThemeManager, runtime themes |
| [windowglow-reference-v2](styling/fluent-ribbon-windowglow-reference-v2.md) | Window glow effects |
| [tabcontrol-reference-v2](styling/fluent-ribbon-tabcontrol-reference-v2.md) | WPF TabControl styling |
| [layout-reference-v2](styling/fluent-ribbon-layout-reference-v2.md) | Spacing, margins, sizing |
| [icon-system-reference](styling/fluent-ribbon-icon-system-reference.md) | Icon handling, DPI awareness |

### Advanced Topics
| Guide | Description |
|-------|-------------|
| [attached-properties-reference](advanced/fluent-ribbon-attached-properties-reference.md) | RibbonProperties class |
| [converters-reference](advanced/fluent-ribbon-converters-reference.md) | Value converters, StaticConverters |
| [state-diagrams-v2](advanced/fluent-ribbon-state-diagrams-v2.md) | Visual state machines |
| [performance-guide](advanced/fluent-ribbon-performance-guide.md) | Large ribbon optimization |

### Reference
| Guide | Description |
|-------|-------------|
| [troubleshooting](reference/fluent-ribbon-troubleshooting.md) | Common issues and solutions |
| [lessons-learned-v2](reference/fluent-ribbon-lessons-learned-v2.md) | Anti-patterns, what NOT to do |
| [wpfanalyzers-reference-v2](reference/wpfanalyzers-reference-v2.md) | WPF code analysis tools |

---

## Features

- **YAML Frontmatter** - All guides have tags and cross-references
- **See Also Links** - Related guides linked in each document
- **Glossary** - Quick term reference
- **"DO NOT DO" Sections** - Anti-patterns to avoid
- **Source References** - Links to actual Fluent.Ribbon source files

---

## Stats

- **49 markdown files** total (37 current + 11 archived + glossary)
- **~29,000 lines** of documentation
- Verified against Fluent.Ribbon source code

---

## Source Material

These guides were created by analyzing:
- [Fluent.Ribbon GitHub](https://github.com/fluentribbon/Fluent.Ribbon)
- Fluent.Ribbon Showcase application
- ControlzEx theming source

---

## Project Tracking

See [PLAN.md](PLAN.md) for project history and session log.

---

## License

MIT License - See [LICENSE](LICENSE)

Fluent.Ribbon is separately licensed under the [MIT License](https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/LICENSE).
