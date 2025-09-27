# STOLON.CLI

The main documentation for the _STOLON.CLI_.

## Command Syntax

To invoke a command, simply type its identifier in the console:

```cs
^[STOLON.CLI]
> ping
pong!
```

### Arguments

Arguments can be passed to commands by appending them after the command identifier seperated by spaces:

```cs
^[STOLON.CLI]
> add 1 1
2
```

> [!WARNING]
> If an argument starts with a dash (e.g., `-2`), specify it after a `--` to > prevent it from being treated as a flag:
>
> ```cs
> ^[STOLON.CLI]
> > add 1 -- -2
> -1
> ```

If an argument contains spaces, enclose it in quotes to pass it as a single argument:

```cs
^[STOLON.CLI]
> greet "JT, JTnadrooi"
Hello JT, JTnadrooi!
```

Without quotes, the arguments will be treated separately:

```cs
^[STOLON.CLI]
> greet JT, JTnadrooi
Hello JT,!
```

### Flags

Flags modify how commands are executed. Prefix the flag identifier with `--` to use it.

Flags are passed using their full name, preceded by `--`:

<sub>_(Logs collapsed for sake of conciseness)_</sub>

```cs
^[STOLON.CLI]
> add 1 1 --verbose
/..LOG DATA../
2
```

Some flags support shorthand versions, note the single minus instead of two:

```cs
^[STOLON.CLI]
> add 1 1 -v
/..LOG DATA../
2
```

> [!TIP]
> Flags in the user.json\* globalFlags array are always appended to the command, even with startup arguments.

<sub>_You can find a list of available flags [here](#all-flags)._</sub>

### Default variables

Default variables are automatically filled in when omitted. Note this fictional `multiply` command accepts two values, the first one of which defaults to `2` while the second parameter defaults to `5`:

```cs
^[STOLON.CLI]
> multiply 5
25
```

You can also explicitly force the default value to be used by using an underscore (`_`). In the following example the first parameter defaults to 2:

```cs
^[STOLON.CLI]
> multiply _ 2
4
```

> [!WARNING]
> In addition to values starting with a hyphen (`-`), the following parameter values have specific restrictions.
>
> -   `--` can only be used after a previous `--`.
> -   `_` is invalid and cannot be used.

## All commands

### `help[?, h] string:filter(default_value:NULL)`

_Display help._

When used without arguments the `help` command displays all loaded commands _(and cmd-namespaces)_.

```r
<command_name>[<prefixes>] <<argument_type>:<argument_name>> # <command_desc>

cli-bump # Bump the .cli-version to .version.
cli-exit # Exit the program.
cli-version # Print the cli version.
conf-open # Open user.ini file.
count int32:target # Count to a number.
dir # Print the folder where STOLON is located.
dir-open # Open the folder where STOLON is located.
help[?, h] string:filter(default_value:NULL) # Display help.
repo # Print repository link.
repo-open # Open the main repository page on Github.
sl-exit # Exit STOLON.
sl-start # Start STOLON.
sl-version # Print the STOLON version.
```

This selection can be filtered down by passing the name of the command or namespace as the first parameter. Do keep in mind that commands take precedence **over** namespaces. To only target namespace suffix the filter argument with a backslash (`\`).

---

_(In depth details for the rest of the commands pending. I am hesitant to write them as the list is still rapidly undergoing changes. Before the next release these docs will be complete.)_

## All flags

_(Pending)_

<sub>_Docs Todo; add list of avalible commands, add list of avalible flags._</sub>
