# About this

At the beginning, all rationales are similar to:

```shell
{value}% of your {issueCategory} was spent in `{symbol}`, We expected this value to be {relation} {criteria}%.
```

Except there was a mistake in the initial data fill up that the value and criteria are flipped like this:

```shell
{value}% of your {issueCategory} was spent in `{symbol}`, We expected this value to be {relation} {criteria}%.
```

This is the console application to fix it.

## Caveats

Technically, it is possible for code 

## How the apps configured

* Register a console app (opi-console)
  * In `API permissions`, add a permission to use `opi-web/basic_info`;
* In `opi-web`, expose the api to `opi-console` by add the client application in `Expose an API`;

Then, use the client code for auth.