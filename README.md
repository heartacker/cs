# cscript - An unix cli tool to execute arbitrary CSharp code

Usage:
```
cscript <csharp code>
```

Example:
```
cat cs.csproj |  cscript "var x = 10; Echo(x + arg.Split('<')[1]);"
```