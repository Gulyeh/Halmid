# Halmid
Chat application inspired on Discord.

Application is based on SignalR system, WebApi and MySql server.
Connection to WebApi is protected with JWT Token Auth. 
Messages are encrypted with AES(Key + IV) and changed to base64 for easier database storage. Every message has own decryption key which is required for message to be readable.

![Main Window](https://i.imgur.com/Y0wbUL8.png)
![Chat](https://i.imgur.com/x4EzlEM.png)
![Settings](https://i.imgur.com/f0oFTQz.png)
