# ⚙️ MarrowConfig

![teamLogo](./assets/teamLogo.jpg)

A CLI-tool to automaticly create and upload configuration file for [Marrow](https://github.com/Skeleton-Army/Marrow) FTC Library by SkeletonArmy#23643

---
## 📦 Download the latest release

- **Windows 64bit**: [MarrowConfig-win64.exe](https://github.com/Skeleton-Army/MarrowConfig/releases/latest/download/MarrowConfig-win64.exe) 
- **Windows 64bit self-contained:** [MarrowConfig-win64_SC.exe](https://github.com/Skeleton-Army/MarrowConfig/releases/latest/download/MarrowConfig-win64_SC.exe) 
- **Windows 32bit**: [MarrowConfig-win32.exe](https://github.com/Skeleton-Army/MarrowConfig/releases/latest/download/MarrowConfig-win32.exe) 
- **Linux**: [MarrowConfig-linux64](https://github.com/Skeleton-Army/MarrowConfig/releases/latest/download/MarrowConfig-linux64) 
- **macOS**: Coming soon 🍎

## 🔧 Usage
Replace `<executable_name>` with the file you downloaded.

Connect to the Robot using ADB and selecting the FTP preset

``` bash
./<executable_name> --adb_ip 191.168.x.x --adb_port 5555 -p f
```
 Display help message 

 ```bash
./<executable_name> --help
 ```

 ---
## ⚠️ Warning 
FTP is an **insecrue** protocol. 

**DO NOT** use a password you use for other accounts are services, make sure to turn off the FTP server after use. 

## ⭐ Contributingand
All contributing are welcome! Feel free to sumbit a Pull Request.

## ✒️ Licence
This project is licensed under the MIT License. See the LICENSE file for more information.

