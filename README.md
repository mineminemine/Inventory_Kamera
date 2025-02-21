# Inventory Kamera - A Genshin Data Scanner

Fan-made Genshin Impact tool that scans your Characters, Weapons, Artifacts, Materials, and Character Development items in your Inventory using the OCR technique.

This scanner exports in `.GOOD`, a JSON-based exporting format, which allows you to use it with compatible online Genshin Impact tools. These tools include artifact optimizing tools including [Genshin Optimizer](https://frzyc.github.io/genshin-optimizer/#/), [SEELIE.me](https://seelie.me/), and [Aspirine's Genshin Impact Calculator](https://genshin.aspirine.su/).

## DISCORD
https://discord.gg/zh56aVWe3U<br>

> **Note**<br>
> Please **read the following instructions carefully** and setup before using the scanner.

## Table of Contents

- Getting Started
  - [Installing Inventory Kamera](#installing-inventory-kamera)
  - [Setting up your Genshin Impact](#set-up-with-your-genshin-impact)
  - [Settings and configurations](#how-to-configure-inventory-kamera)
  - [Run Inventory Kamera](#how-to-run-inventory-kamera)
- Scanner
  - [How to update Inventory Kamera's database](#updating-for-new-game-versions)
- Repository
  - [Report a Bug / Scanning Issues](#report-an-issue)
  - [Request a new feature](#request-a-new-feature)
  - [Ask a question](#ask-a-question)
  - [Frequently Asked Questions (FAQ)](#frequently-asked-questions-faq)
  - [Licenses](#license)

## Installing Inventory Kamera

For Inventory Kamera, we asked you to have **these following things installed on your device**:

- [GenshinImpact.exe](https://genshin.hoyoverse.com/) or [YuanShen.exe](https://ys.mihoyo.com/) launcher
- [Microsoft Visual C++ Redistributable for Visual Studio 2015-2022 (x86 or x64)](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#visual-studio-2015-2017-2019-and-2022)
- Latest version of [Inventory Kamera](https://github.com/Andrewthe13th/Inventory_Kamera/releases)

To install, unzip the downloaded release to a folder of your choosing and open the launcher. This will likely prompt you for Security or/and User Access. This access is required to interact with the Genshin Impact application itself.

To upgrade from a previous version, unzip the downloaded release to either the current location you have Inventory Kamera at or to a new location of your choosing.

> **Warning**<br>
> Due to the way Inventory Kamera is compiled, application settings that you change from default (most importantly the export folder!) will be reset to default values. It is recommended to continue to use one folder location when unzipping future releases. More information can be [found here](https://github.com/Andrewthe13th/Inventory_Kamera/issues/301#issuecomment-1179611917)

## Set up with your Genshin Impact

1. Log in to Genshin Impact and click start.
2. Open [Paimon menu](https://genshin-impact.fandom.com/wiki/Paimon_Menu). (Default shortcut: `ESC`)
3. Go to `Settings` (Cog Icon ⚙) and set it to these settings:
   - Under `Languages`, set _Game Language_ to English.
   - Under `Graphics`, set _Display Mode_ to _any windowed resolution_ that is _16:9 or 16:10 ratio_. Make sure that the entire game window is visible on the screen.
     - Example of 16:9 includes 1920x1080 (Full HD), 3840x2160 (4K), 2560x1440 (2K).
     - Example of 16:10 includes 1920x1200.
     - Don't know if a resolution is 16:9 or 16:10? [Find out here](https://andrew.hedges.name/experiments/aspect_ratio/).
     - > **Warning**<br>
       > If you have an ultrawide monitor please see [this thread](https://github.com/Andrewthe13th/Inventory_Kamera/issues/40)
   - Under `Controls`, set _Control Type_ to Keyboard.
     - If you have rebound the inventory key (default: B) or character screen key (default: C), either revert your binding to default or set up new key binding in Inventory Kamera.

## How to Configure Inventory Kamera

Before starting the scanner, you can (optionally) choose to edit these following configurations:

- Configure which category (Weapons, Artifacts, Characters, Items) wanted to be scanned.
- Configure minimum weapon and artifact rarity to be scanned.
- Set the scanner delays to slow Inventory Kamera's scanning speed if you might not have a strong PC
- Set the file export destination in the File Directory

## How to Run Inventory Kamera

Starts the Inventory Kamera scans by **leaving the game screen with the Paimon Menu stays open** and click 'Scan' to start scanning.

> **Warning**<br>
> While scanning, **do not use your mouse or keyboard**. The scanner uses your keyboard and mouse input to automate scanning.
>
> If you like to terminate the scan, press the `ENTER` button at any time. However, the application will not output any scanned results. You may press the 'Export Scanned Data' button to force an export of data that was collected in the most previous scan (complete or incomplete).

## Report an Issue

If you ran into a problem with our scanner (e.g. bug, app crash, invalid export format), please [create your issue here](https://github.com/Andrewthe13th/Inventory_Kamera/issues/new/choose) and try to fill it out as much as possible. Itself, along with the evidence, will greatly speed up the bug fix process(es).

> **Note**<br>
> Before submitting an issue, **please [check that there are no other similar issues](https://github.com/Andrewthe13th/Inventory_Kamera/issues?q=is%3Aissue) that is similar to you, especially ones that are still open.**<br>
> Start by leaving a reaction emoji to that issue (more reacts means more dev attention!). Please try to limit comments to new or helpful information (i.e not "Same issue here" comments). You can choose to _subscribe_ to that issue by clicking 'Subscribe' in the Notifications section to get notifications on thread developments.

### About writing a new Issue

We would **love to have Screenshots (especially video recordings!) and Error Logs as evidence**. These can be very helpful in debugging your issue. Add it to the issue by drag-and-drop or attatching the file to the issue template. Inventory Kamera may screenshots under the `logging` folder divided in to categories when it might have encountered an issue. Zipping up the `logging` folder is the best way to submit your logs. You may check the 'Log All Screenshots' box to force this behavior on most areas that may be cause for concern to Devs.

## Request a new feature

If you like to request a new feature, please visit the [Discussion forum](https://github.com/Andrewthe13th/Inventory_Kamera/discussions) before opening a new feature request using [Inventory Kamera's Feature Request form](https://github.com/Andrewthe13th/Inventory_Kamera/discussions/new?category=ideas-or-feature-requests).

## Ask a question

General questions? Start by looking in [Inventory Kamera's Discussion forum](https://github.com/Andrewthe13th/Inventory_Kamera/discussions).
If you have a question that doesn't have a thread, you may start a new [General thread](https://github.com/Andrewthe13th/Inventory_Kamera/discussions/new?category=general) or [Q&A thread](https://github.com/Andrewthe13th/Inventory_Kamera/discussions/new?category=q-a).

## Updating for new game versions

Inventory Kamera uses lists of valid items and characters to assist with text recognition. These lists are kept locally in the `inventorylists` folder. These lists must be updated with every new version of the game and can be updated both automatically and manually.

### Updating Automatically

If an update window does not show up when starting the application, you can use the `Update Lookup Tables` under `Options` to run the updater. You may optionally force the updater to run should it not detect a new Genshin version. Inventory Kamera uses [Dimbreath's Genshin Data Github Repo](https://github.com/Dimbreath/GenshinData) to keep data updated. A big thanks for all the hard work done there.

### Updating Manually

All lists, with the exception of characters, are kept in a simple key:value JSON-readable format. 'value' is the name of an item in [PascalCase](<https://en.wikipedia.org/wiki/Naming_convention_(programming)#Examples_of_multiple-word_identifier_formats>) and 'key' is whatever 'value' is only in all lowercase. `materialscomplete.json` is the combination of `devmaterials.json` and `materials.json` so updating either of those requires an update to `materialscomplete.json` as well. The format for manually updating characters is slightly different. The key for a character is still the lowercase version of the character's name in PascalCase. The value is in the following format:

```json
{
  "GOOD": "SangonomiyaKokomi",
  "ConstellationOrder": ["burst", "skill"],
  "WeaponType": 4
}
```

The character's name is as it appears in the party menu on the right side of the in-world UI or the character's menu screen in the top left corner. The constellation order depends on which talent the third constellation upgrades for each character. The weapon type values are as follows:

0 = Sword, 1 = Claymore, 2 = Polearm, 3 = Bow, 4 = Catalyst

Consider using [a JSON text validator](https://jsonlint.com/) after following this manual method. Support may or may not be provided if you choose to go this route.

## Frequently Asked Questions (FAQ)

#### Can Inventory Kamera get me banned?

According to HoYoverse's [response to Script, Plug-In, and Third-Party Software](https://genshin.hoyoverse.com/en/news/detail/5763), we believe not.

The scanner does not provide any exploits or game progression. It only takes screenshots of a portion of the game window, processes the image, and allows you to control the exported data. The account will still be yours to keep. We do not provide any account exchanges or top-ups for Primogem.

In addition, we have not received any warnings about the application's development.
However, that does not mean it will stay that way forever; We are at the mercy of HoYoverse.

## License

- This project is under the [MIT license](LICENSE).
- This project uses third-party libraries or other resources that may be
  distributed under different licenses.

---

All rights reserved by © Cognosphere Pte. Ltd. This project is not affiliated with, nor endorsed by HoYoverse. Genshin Impact™ and other properties belong to their respective owners.
