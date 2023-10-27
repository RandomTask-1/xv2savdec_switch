# xv2savdec_switch for the Switch

## Description

A save decryptor/encryptor for the Switch version of DBXV2

## Requirements

- Your DBXV2 `savefile1.dat` file
- .NET Framework ver 4.8 or above

## Instructions
- Drag an encrypted file into the program and it will create a ".switch.sav.dec" and ".pc.sav.dec" file with the decrypted content.  The pc.save.dec file mimics the pc format files and can be opened with lazybone's editor.  It cannot be used to convert a save from Switch to PC format.
- Drag a decrypted file onto the program and it will create an encrypted ".dat" file that can be returned to the Switch

## Credits
- **Eternity** - Huge thanks goes to Eternity for his/her initial work on a DBXV2 save file decryptor/encryptor ([xv2savdec](http://animegamemods.net/thread/701/tools-eternity)) for PS4/XBone/PC
- **Falo** - For converting the [xv2savdec](https://gbatemp.net/threads/is-xenoverse-2-save-encryption-being-worked-on-by-anyone.512671/#post-8209053) for the Switch, he/she converted the decryption function while I converted the encryption function
- **mineminemine** - For the source I iterated on [xv2savdec](https://github.com/mineminemine/xv2savdec_switch)

Falo was also the one who found out that the Switch version does not contain an MD5 header.
