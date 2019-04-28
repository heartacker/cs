# cs - An unix cli tool to execute arbitrary C# code

Usage:
```
cs <csharp code>
```

Example:
```
cat cs.csproj |  cs "var x = 10; Echo(x + arg.Split('<')[1]);"
```
