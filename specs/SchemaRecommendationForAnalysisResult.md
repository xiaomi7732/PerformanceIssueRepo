# Schema Recommendation for Analysis Result

## Descriptions

It is recommended to have these properties to be your minimal set of an issue found by analysis:

* UniqueID: string
  * An code to link to a known issue in the [Issue Registry](). It is a compound id of [Issue Type](./IssueType.md), and a number for the problem. Your analyzer should understand and properly output the issue record with an issue id in the [Issue Registry](). For example, `C0003` means issue number 3 on CPU; and `M0005` means issue #5 on memory.

* CallstackContext: string[]?
  * The callstack to the issue.
    * It is recommended to have around 10 frames in the call stack to provide enough context.

* SymbolIndex - int?
  * 0-based index for the frame in the callstack directly link to the issue.

* Criteria - double
  * The benchmark metrics for the system to hit.

* Value - double
  * The actual value of the metric to compare with the `criteria`.

* Relation - string
  * The relationship between the value and the criteria. It is recommended to use it in form: `Value` `Relation` `Criteria`. For example: `3% (value) of your CPU was spent in 'abc', we expected this value to be >= (relation) 1%(criteria)`.
  * In other words, value on the left of the relation, criteria on the right hand side.

## Extending the schema

* The schema above is recommended as a minimal property set. To provide more info to the end user, you can extend the schema based on your needs. For example, you want to display a timestamp when the issue happens, add a property of `Timestamp`.

    [![A diagram](https://mermaid.ink/img/pako:eNplUTtPwzAQ_iuWV1KJlQiJIWXIUAmRwoIZrvG1tepHsc9SQul_x3FSiJQbbOt7nO-zL7x1EnnJWw0hrBUcPBhhWaoX9Ps6hIjs8We1Ypt-AJw3YFvM-CjLxn_xZUSHugvklT2wN6u-ItZywXx8sgq0DgTtqXKWsKOZRll6Yk1vdk7XVmI3o6SLO42s8orQK1gy76Bv880neUUNpJwdmes8wDLePMkaCLfKIBuWNK85Ty14wQ0mk5LpDbNDcDqiQcHLdJTgT4IXI26g26aIjfrO7MN9LmGHJvEs0xXPUpHzvNyDDlhwiOSa3ra8JB_xJpo-6U-F2bSZvnHYrr--_pej?type=png)](https://mermaid.live/edit#pako:eNplUTtPwzAQ_iuWV1KJlQiJIWXIUAmRwoIZrvG1tepHsc9SQul_x3FSiJQbbOt7nO-zL7x1EnnJWw0hrBUcPBhhWaoX9Ps6hIjs8We1Ypt-AJw3YFvM-CjLxn_xZUSHugvklT2wN6u-ItZywXx8sgq0DgTtqXKWsKOZRll6Yk1vdk7XVmI3o6SLO42s8orQK1gy76Bv880neUUNpJwdmes8wDLePMkaCLfKIBuWNK85Ty14wQ0mk5LpDbNDcDqiQcHLdJTgT4IXI26g26aIjfrO7MN9LmGHJvEs0xXPUpHzvNyDDlhwiOSa3ra8JB_xJpo-6U-F2bSZvnHYrr--_pej)


## Why the minimal set of the properties?

* These properties provides semantics, so that they could be leveraged to fill up placeholders in descriptions or recommendations of an performance issue. For example, consider the following description:

    ```markdown
    {value}% of your [issueType] was spent in `{functionName}`, we expected this value to be {relation} {criteria}.
    ```

    If we map up the content in the `{}`, we will be able to get a reasonable reading like this, in English:

    ```markdown
    3% of your CPU was spent in `String.Concat`, we expected this value to be <= 1%.
    ```

* Now, if the client system is willing to localize it, the result could look like:

    ```
    `String.Concat`占用了3%的CPU，正常预期应该 <= 1%。
    ```

## SDKs

* The plan is to provide SDKs to provide functions to fill up the placeholders for `.NET` application.
* //TBD: Provide a simple SDK for js?

## What should NOT be in the analysis result

* You could easily extend the `PerfIssue`, it is recommended to avoid adding these things:

  * Plain Text that you don't want to localize - providing it in the issue causes difficulties to localization.
  * Anything that is will be provided by associated issue id.

