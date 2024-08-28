# Description

Package provides base classes for integration tests. 
`BaseFunctionIntegrationTest<TFunction>`
`HttpFunctionIntegrationTest<TFunction>`


# Example
```
    public class FunctionIntegrationTest : HttpFunctionIntegrationTest<Function>
    {
        public FunctionIntegrationTest(LuebenWireMockClassFixture LuebenWireMockClassFixture, ITestOutputHelper testOutputHelper)
            : base(LuebenWireMockClassFixture, testOutputHelper, consumer: "Foo")
        {
        }
    }
```