Simple functionality allow using Socket via Socks5 protocol

Example for HTTP request via Socks5 protocol:

string host = "www.google.com";
Socket socket = Socks5Client.Connect("localhost", 8081, host, 80, null, null);

easy isn't it?
Enjoy.