using SapphTools.DHA.Stig.Remediator.TestSled;

namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class TestSledRegressionTests {
    private static TestCase Case(int order = 1) {
        RegistryValueValue desired = TestData.Value(42);
        return new TestCase {
            Order = order,
            Setting = new Setting {
                Order = 0,
                RequiredContext = SettingContext.None,
                Data = desired.Clone()
            },
            ExpectedAfter = desired,
            ExpectedResult = ActionResult.ActionSuccess
        };
    }

    private static TestResult SuccessfulResult(bool? rollbackPass = null) => new() {
        ActualAfter = TestData.Value(42),
        RemediationAction = TestData.Action(),
        RollbackPass = rollbackPass
    };

    [TestMethod]
    public void AutomaticRollbackFailureMustFailTheOriginalTestCase() {
        TestCase testCase = Case();
        testCase.Results["HOST-A"] = SuccessfulResult(false);
        Assert.AreEqual(false, testCase.Pass);
        Assert.AreEqual(false, testCase.Results["HOST-A"]!.RollbackPass);
    }

    [TestMethod]
    public void ExecutionErrorsArePersistedAndFailAssertions() {
        TestCase testCase = Case();
        TestResult result = SuccessfulResult();
        result.Error = "IOException: cannot observe state";
        testCase.Results["HOST-A"] = result;
        Assert.AreEqual(false, testCase.Pass);
    }

    [TestMethod]
    public void CloningACaseDeepClonesItsMachineResults() {
        TestCase original = Case();
        original.Results["HOST-A"] = SuccessfulResult(false);
        TestCase copy = original.Clone();
        Assert.AreNotSame(original.Results, copy.Results);
        Assert.AreNotSame(original.Results["HOST-A"], copy.Results["HOST-A"]);
        Assert.AreNotSame(original.Results["HOST-A"]!.ActualAfter, copy.Results["HOST-A"]!.ActualAfter);
        Assert.AreNotSame(original.Results["HOST-A"]!.RemediationAction, copy.Results["HOST-A"]!.RemediationAction);
        copy.Results["HOST-A"]!.RollbackPass = true;
        Assert.AreEqual(false, original.Results["HOST-A"]!.RollbackPass);
    }

    [TestMethod]
    public void PartialMultiMachineEvidenceCannotPass() {
        Test test = new() {
            TestId = 1, Phase = 1, Order = 1,
            Config = new TestConfig(),
            TestRun = true,
            RunTargets = ["HOST-A", "HOST-B"]
        };
        TestCase testCase = Case();
        testCase.Results["HOST-A"] = SuccessfulResult();
        test.TestSuite.Add(testCase);
        Assert.AreEqual(true, testCase.Pass);
        Assert.AreEqual(false, test.TestPass);
        Assert.IsFalse(test.Pass);

        testCase.Results["HOST-B"] = SuccessfulResult();
        Assert.AreEqual(true, test.TestPass);
        Assert.IsTrue(test.Pass);
    }

    [TestMethod]
    public void RecoveryRequiresEveryTargetToHaveAnOutcome() {
        Test test = new() {
            TestId = 2, Phase = 1, Order = 2,
            Config = new TestConfig(),
            TestRun = true,
            RunTargets = ["HOST-A", "HOST-B"]
        };
        TestCase main = Case();
        main.Results["HOST-A"] = SuccessfulResult();
        main.Results["HOST-B"] = SuccessfulResult();
        test.TestSuite.Add(main);

        TestCase recovery = Case();
        recovery.Results["HOST-A"] = SuccessfulResult();
        test.RecoverySuite.Add(recovery);
        Assert.AreEqual(false, test.RecoveryPass);
        Assert.IsFalse(test.Pass);

        recovery.Results["HOST-B"] = SuccessfulResult();
        Assert.AreEqual(true, test.RecoveryPass);
        Assert.IsTrue(test.Pass);
    }

    [TestMethod]
    public void ResultTargetLookupIsCaseInsensitiveAfterAssignment() {
        TestCase testCase = Case();
        testCase.Results = new Dictionary<string, TestResult?> {
            ["HOST-A"] = SuccessfulResult()
        };
        Assert.IsTrue(testCase.Results.ContainsKey("host-a"));
    }

    [TestMethod]
    public void CloneWithoutResultsClearsExecutionTargetCoverage() {
        Test test = new() {
            TestId = 3, Phase = 2, Order = 1,
            Config = new TestConfig(),
            TestRun = true,
            RunTargets = ["HOST-A"]
        };
        TestCase testCase = Case();
        testCase.Results["HOST-A"] = SuccessfulResult();
        test.TestSuite.Add(testCase);

        Test reset = test.Clone(withoutResults: true);
        Assert.IsFalse(reset.TestRun);
        Assert.AreEqual(0, reset.RunTargets.Count);
        Assert.AreEqual(0, reset.TestSuite.Single().Results.Count);
        Assert.IsFalse(reset.Pass);
    }
}
