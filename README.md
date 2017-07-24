# Docpal
[![Build status](https://ci.appveyor.com/api/projects/status/dlvs655kyryk0doy?svg=true)](https://ci.appveyor.com/project/TheBerkin/docpal)

**Are overcomplicated documentation tools making you headbang a brick wall?**

**Wishing for something compatible with a Markdown-based site generator?**

**Longing for more than an XML comment scraper?**

**Docpal** is a Markdown documentation generator for .NET assemblies without all the bloat.
It's simple, straightforward, and easy to use.

Take your EXE or DLL and do this:

```
docpal Rant.dll -out ./docs
```

Boom, you have your docs. *It's that easy!*

## What Docpal offers

### Get started in minutes instead of hours
Don't waste your time learning some other tool that takes forever to set up.
You can learn Docpal in a couple minutes and have results in seconds!

### No gigantic config files
Docpal works with the very basics. You don't need config files with hundreds of settings.

### XML is optional
You don't need an XML documentation file to use Docpal.
If there is one, its contents will be automatically extracted into the docs.
**No extra setup required.**

### Show only what's important
Unlike some other docs generators, Docpal only looks at public types and members. 

### Two output styles
Many pages or one -- it's up to you!
Tell Docpal to combine all your docs on one page with the `--slim` flag.

### Easy to integrate
Docpal works wonderfully with other Markdown-based website generators, such as MkDocs.

## How to use

Docpal can be run from a terminal or integrated into your build process to run automatically.

Here are the options it can currently use:

|Option|Description|
|------|-----------|
|`-out [path]`|Specifies the path where the docs will be saved.|
|`-xml [path]`|Specifies a custom XML documentation path.|
|`--slim`|Specifies that the docs will be combined into a single .md file.|
|`--noxml`|Don't use XML.|

## Compiling

Docpal is written in C# 7 and requires Visual Studio 2017 to compile.