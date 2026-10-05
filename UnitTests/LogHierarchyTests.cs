using SapphTools.DHA.Stig.Remediator.Classes.Rollback;

namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class LogHierarchyTests {
    [TestMethod] public void FlattenRetainsEveryActionAcrossBatchesMachinesRulesAndSettings() {
        RemediationAction first = TestData.Action(); LogBatchCollection logs = new(first);
        RemediationAction same = TestData.Action(first.ToPreAction()); same.ActionNumber++;
        RemediationAction setting = TestData.Action(first.ToPreAction()); setting.SettingIndex++;
        RemediationAction rule = TestData.Action(first.ToPreAction()); rule.RuleId = "V-456";
        RemediationAction machine = TestData.Action(first.ToPreAction()); machine.ComputerName = "other-host";
        RemediationAction batch = TestData.Action();
        foreach (RemediationAction action in new[] { same, setting, rule, machine, batch }) logs.Add(action);
        List<RemediationAction> actual = logs.Flatten().Select(a => a.ToRemediationAction()).ToList();
        CollectionAssert.AreEquivalent(new[] { first, same, setting, rule, machine, batch }, actual);
        Assert.AreEqual(6, actual.Distinct(ReferenceEqualityComparer.Instance).Count());
    }
    [TestMethod] public void RepeatedFlattenIsReadOnlyAndDoesNotDuplicateEntries() {
        LogBatchCollection logs = new(TestData.Action());
        Assert.AreEqual(1, logs.Flatten().Count()); Assert.AreEqual(1, logs.Flatten().Count());
    }
    [TestMethod] public void BatchRejectsWrongRemediationBatch() {
        RemediationAction first = TestData.Action(); LogBatch batch = new(first);
        RemediationAction wrong = TestData.Action(first.ToPreAction()); wrong.RemediationBatch = Guid.NewGuid();
        Assert.ThrowsException<ArgumentException>(() => batch.Add(wrong)); Assert.AreEqual(1, batch.MachineLogs.Count);
    }
    [TestMethod] public void MachineRejectsDifferentHostOrBatch() {
        RemediationAction first = TestData.Action(); LogMachine machine = new(first);
        RemediationAction wrong = TestData.Action(first.ToPreAction()); wrong.ComputerName = "different";
        Assert.ThrowsException<ArgumentException>(() => machine.Add(wrong));
        wrong.ComputerName = first.ComputerName; wrong.RemediationBatch = Guid.NewGuid();
        Assert.ThrowsException<ArgumentException>(() => machine.Add(wrong));
    }
    [TestMethod] public void RuleRejectsDifferentRuleHostOrBatch() {
        RemediationAction first = TestData.Action(); LogRule rule = new(first);
        foreach (string field in new[] { "rule", "host", "batch" }) {
            RemediationAction wrong = TestData.Action(first.ToPreAction());
            if (field == "rule") wrong.RuleId = "V-456";
            if (field == "host") wrong.ComputerName = "other";
            if (field == "batch") wrong.RemediationBatch = Guid.NewGuid();
            Assert.ThrowsException<ArgumentException>(() => rule.Add(wrong));
        }
    }
    [TestMethod] public void SettingRejectsWrongIndexRuleHostOrBatchWithoutAddingAction() {
        RemediationAction first = TestData.Action(); LogSetting setting = new(first);
        foreach (string field in new[] { "setting", "rule", "host", "batch" }) {
            RemediationAction wrong = TestData.Action(first.ToPreAction());
            if (field == "setting") wrong.SettingIndex++;
            if (field == "rule") wrong.RuleId = "V-456";
            if (field == "host") wrong.ComputerName = "other";
            if (field == "batch") wrong.RemediationBatch = Guid.NewGuid();
            Assert.ThrowsException<ArgumentException>(() => setting.Add(wrong));
        }
        Assert.AreEqual(1, setting.Count);
    }
    [TestMethod] public void SettingPreservesEmissionOrderAndActionMetadata() {
        RemediationAction first = TestData.Action(); first.Before = TestData.Value("1", RegistryValueKind.String); first.After = TestData.Value(1);
        LogSetting setting = new(first); RemediationAction second = TestData.Action(first.ToPreAction()); second.ActionNumber++;
        setting.Add(second);
        Assert.AreEqual(first.ActionNumber, setting[0].ActionNumber); Assert.AreEqual(second.ActionNumber, setting[1].ActionNumber);
        LogAction log = setting[0]; Assert.AreSame(first, log.ToRemediationAction());
        Assert.AreEqual(first.Source, log.Source); Assert.AreEqual(first.Result, log.Result); Assert.AreEqual(first.TargetType, log.TargetType);
        Assert.AreEqual(first.RollbackCapability, log.RollbackCapability); Assert.AreEqual(first.Target, log.Target);
        Assert.AreEqual(first.RemediationTimestamp, log.RemediationTimestamp); Assert.AreEqual(first.FailureMessage, log.FailureMessage);
        Assert.AreSame(first.Before, log.Before); Assert.AreSame(first.After, log.After);
    }
    [TestMethod] public void MachineAndRuleCollectionsSupportCaseInsensitiveLookup() {
        RemediationAction first = TestData.Action(); LogMachineCollection machines = new(first); LogRuleCollection rules = new(first);
        Assert.IsTrue(machines.ContainsKey("TEST-HOST")); Assert.IsTrue(machines.TryGetValue("TEST-HOST", out LogMachine? machine));
        Assert.AreEqual(first.ComputerName, machine!.ComputerName);
        Assert.IsTrue(rules.ContainsKey("v-123")); Assert.IsTrue(rules.TryGetValue("v-123", out LogRule? rule));
        Assert.AreEqual(first.RuleId, rule!.RuleId);
        Assert.IsFalse(machines.TryGetValue("missing", out _)); Assert.IsFalse(rules.TryGetValue("missing", out _));
    }
}
