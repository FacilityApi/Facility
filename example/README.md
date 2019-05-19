The [build script](../tools/Build/Build.cs) verifies that `ExampleApi.fsd` and `ExampleApi.fsd.md` both convert to `output/ExampleApi.fsd`.

It also verifies that both files convert to `output/ExampleApi-nowidgets.fsd` when excluding the `widgets` tag.
