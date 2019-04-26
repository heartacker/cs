# csharp - An unix cli tool to execute arbitrary CSharp code

Usage:
```
csharp <csharp code>
```

Example:
```
cat cs.csproj |  csharp "var x = 10; Echo(x + arg.Split('<')[1]);"
```