# PerfIssue Registry

## Description

This is the central of this repo - a registry for performance issues. Each performance is associated with a unique id.

## The schema

The registry is a collection of `Perf Issue Registry Entries`, it is a collection.

Here's the schema of each item in the registry:

* `supportedTypes`
  * An array of possible types for the issue. Refer to [issue types](./IssueType.md) for more details.

* `isActive`
  * A boolean value, indicating whether it is an active registry entry. When it is active, it will be used to generate issue list.


* `issueId`
  * An int. This is a unique id for a given issue. Notice that a same issue could be applied to different types - like for CPU or for Memory, they will have the same issue id. And there will be a compound unique id like `C0001` for issue #1 on CPU, and `M0001` for issue #1 on memory, and/or `T0001` for issue that concerns Thread/Thread pool.

* `title`
  * A relatively short string for user to glance where the issue is. For example: Allocations in `Equals`/`GetHashCode`, or "Inefficient string concatenation".

* `description`
  * A string to expand on title, describe what the issue is. For example: Hot-path methods like `Equals`/`GetHashCode` should be kept allocation free. LINQ methods tend to allocate enumerators on the Heap, while `string.Format` leads to string allocations.

* `docURL`, optional
  * A url linking to a web page for instructions to fix the issue.

* `recommendation`
  * A quick guide on how to fixing the issue. For example:
    ```
    This can be fixed by removing calls to such methods. LINQ queries can be unrolled into loops. `string.Format` can be replaced with a comparison of it's component strings.
    ```
    Or
    ```
    Refer to the documentation for a fix.
    ```

* `rationale`
  * A description of why the analyzer treat it as an performance issue. For example:

    ```shell
    {criteria}% of your [issueType] was spent in \u0060{symbol}\u0060, We expected this value to be {relation} {value}%.
    ```

## The semantics

Placeholders can be used for these fields: title, description, recommendation, rationale. For example:

```
{criteria}% of your [issueType] was spent in \u0060{symbol}\u0060, We expected this value to be {relation} {value}%.
```

The client are expected to replace those placeholders in curly braces `{}` with proper values on runtime. Refer to [AnalyzerIssue](./specs/SchemaRecommendationForAnalysisResult.md) for how to implement it.
