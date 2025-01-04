namespace CoursesManager.MVVM.Env.Tests
{
    public class TestModel
    {
        public string Key1;
        public int Key2;
        public bool Key3;
    }

    public class TestModel2 : TestModel
    {
        public bool Key6;
    }

    public class EnvManagerTests
    {
        private string _testDirectory;

        public void NonAutomaticSetup()
        {
            EnvManager<TestModel>.Reset();
            _testDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CoursesManagerTest");

            Directory.CreateDirectory(_testDirectory);

            var envFilePath = Path.Combine(_testDirectory, ".env");
            File.WriteAllLines(envFilePath, new[]
            {
                "Key1=Value1",
                "Key2=42",
                "Key3=true",
            });

            EnvManager<TestModel>.EnvFolderPath = _testDirectory;
            EnvManager<TestModel2>.EnvFolderPath = _testDirectory;
            Directory.CreateDirectory(_testDirectory);
            Directory.SetCurrentDirectory(_testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test, Order(2)]
        public void Initialization_ShouldLoadValuesFromEnvFile()
        {
            NonAutomaticSetup();

            var values = EnvManager<TestModel>.Values;

            Assert.That("Value1", Is.EqualTo(values.Key1));
            Assert.That(42, Is.EqualTo(values.Key2));
            Assert.That(values.Key3, Is.True);
        }

        [Test, Order(3)]
        public void Initialization_ShouldThrowExceptionForMissingKey()
        {
            NonAutomaticSetup();

            var ex = Assert.Throws<Exception>(() =>
            {
                var a = EnvManager<TestModel2>.Values;
            });
        }

        [Test, Order(4)]
        public void Save_ShouldWriteFieldsToEnvFile()
        {
            NonAutomaticSetup();

            var values = EnvManager<TestModel>.Values;
            values.Key1 = "NewValue1";
            values.Key2 = 99;
            values.Key3 = false;

            EnvManager<TestModel>.Save();

            var envFilePath = Path.Combine(_testDirectory, ".env");
            Assert.That(File.Exists(envFilePath), Is.True);
            var content = File.ReadAllLines(envFilePath);
            Assert.That(content.Contains("Key1=NewValue1"), Is.True);
            Assert.That(content.Contains("Key2=99"), Is.True);
            Assert.That(content.Contains("Key3=False"), Is.True);
        }

        [Test, Order(1)]
        public void FindEnvFiles_ShouldReturnEmptyListIfNoFilesExist()
        {
            var result = EnvManager<TestModel>.Values;

            Assert.That(result.Key1, Is.Default);
            Assert.That(result.Key2, Is.Default);
            Assert.That(result.Key3, Is.Default);
        }
    }
}
