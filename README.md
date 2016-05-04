## QIRC
An extensible IRC Bot framework, written in CSharp and .NET 4.5

### What is QIRC
As previously said, QIRC is a framework for an IRC bot, written in CSharp. It is highly modular, the core just connects to the
IRC server and invokes functions in Plugins, loaded from .dll files. There are the definitions for the commands, or other custom
actions. You can simply adjust the toolset of your bot: Either add new dll files with own functions and remove the ones you don't need!

### How do I run QIRC?
To setup a QIRC bot, download the contents of this repository to a webserver that has mono 4 or the .NET Framework 4.5 installed.
You can build the solution file either with xbuild, msbuild, MonoDevelop or VisualStudio. Your QIRC instance will end up in the 
'Distribution/' folder, you can copy it to any point of your harddrive. Now configure your plugin setup, and start the .exe file with
`mono QIRC.exe`, but kill it immideately. This has to happen in order to generate default configs inside of the settings folder.
Adjust them to match your irc server and prefered bot name. Then start the exe again. Thats it!

### Example config
settings.json:
```json
{
  "name": "QIRC-Bot",
  "control": "!",
  "saveInterval": 10
}
```

connection.json:
```json
{
  "host": "irc.freenode.net",
  "port": 6667,
  "useSSL": false,
  "password": "",
  "channels": [
    {
      "name": "#botwar",
      "password": "",
      "serious": false
    }
  ],
  "admins": [
    {
      "name": "Thomas",
      "root": true // Roots have more access than normal admins
    }
  ]
}
```

### Modules
There are lots of modules available in QIRC:
* acr, acronym - Expands acronyms into their explanations
* action - Makes the bot post a /me into the channels
* alias - Creates an alias for a more complex command, example !run gets to !action runs
* choose - Picks one option between multiple ones.
* csharp - Evaluates CSharp using Monos Mono.CSharp library
* s/// - Corrects another user (or yourself) using the unix sed syntax
* github - provides automated issue or pull request expansion using the github api
* g, google - googles a term and returns the first result
* help - Displays a help text for a programm or provides a list of them
* join, leave - Makes the bot join or leave an irc channel
* logging - QIRC does not log anything on it's own. :)
* nickserv - Provides authentication with NickServ
* psa - Posts a public service announcement to all channels the bot has joined
* roll - Rolls dice
* say - Same as action, but as plain text instead of /me
* seen - Returns the last message of a user
* tell - Saves a message for the user that gets delivered when he talks
* wa, wolfram - Queries WolframAlpha

### License
My code is licensed as MIT
If you want to contribute under a different license, we will see what we can do.
Pull requests are always welcome.