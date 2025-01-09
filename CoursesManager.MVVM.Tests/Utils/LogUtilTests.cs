//using NUnit.Framework.Internal;
//using System.Reflection;

//namespace CoursesManager.MVVM.Tests.Utils
//{
//    [TestFixture]
//    public class LogUtilTests
//    {
//        private string _logFilePath;

//        [SetUp]
//        public void Setup()
//        {
//            string logDirectory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "", "");
//            _logFilePath = Path.Combine(logDirectory, "application.log");

//            // Ensure we start with a fresh and clean log file.
//            if (File.Exists(_logFilePath))
//                File.Delete(_logFilePath);
//        }

//        [TearDown]
//        public void Cleanup()
//        {
//            if (File.Exists(_logFilePath))
//                File.Delete(_logFilePath);
//        }

//        [Test]
//        public void Log_Message_Should_Contain_Info_Prefix()
//        {
//            string testMessage = "Info test message";

//            LogUtil.Info(testMessage);

//            Assert.That(File.Exists(_logFilePath), Is.True, "Log file should be created.");
//            string content = File.ReadAllText(_logFilePath);
//            Assert.That(content, Does.Contain("[Info]").And.Contain(testMessage));
//        }

//        [Test]
//        public void Log_Message_Should_Contain_Warning_Prefix()
//        {
//            string testMessage = "Warning test message";

//            LogUtil.Warning(testMessage);

//            Assert.That(File.Exists(_logFilePath), Is.True, "Log file should be created.");
//            string content = File.ReadAllText(_logFilePath);
//            Assert.That(content, Does.Contain("[Warning]").And.Contain(testMessage));
//        }

//        [Test]
//        public void Log_Message_Should_Contain_Error_Prefix()
//        {
//            string testMessage = "Error test message";

//            LogUtil.Error(testMessage);

//            Assert.That(File.Exists(_logFilePath), Is.True, "Log file should be created.");
//            string content = File.ReadAllText(_logFilePath);
//            Assert.That(content, Does.Contain("[Error]").And.Contain(testMessage));
//        }

//        [Test]
//        public void Log_MultipleMessages_Should_Be_Appended()
//        {
//            string infoMessage = "Info message";
//            string warningMessage = "Warning message";
//            string errorMessage = "Error message";

//            LogUtil.Info(infoMessage);
//            LogUtil.Warning(warningMessage);
//            LogUtil.Error(errorMessage);

//            Assert.That(File.Exists(_logFilePath), Is.True, "Log file should be created after multiple writes.");
//            string content = File.ReadAllText(_logFilePath);

//            // The content should contain each message in the order they were logged.
//            string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
//            Assert.That(lines.Length, Is.EqualTo(3), "Expected three lines in the log file.");

//            // Check the messages are in the correct log lines
//            Assert.That(lines[0], Does.Contain("[Info]").And.Contain(infoMessage));
//            Assert.That(lines[1], Does.Contain("[Warning]").And.Contain(warningMessage));
//            Assert.That(lines[2], Does.Contain("[Error]").And.Contain(errorMessage));
//        }

//        [Test]
//        public void Log_Message_Should_Contain_TimeStamp_In_Expected_Format()
//        {
//            string testMessage = "Timestamp test message";

//            LogUtil.Info(testMessage);

//            string content = File.ReadAllText(_logFilePath);

//            string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
//            Assert.That(lines.Length, Is.GreaterThanOrEqualTo(1), "There should be at least one log entry.");

//            string firstLine = lines[0];
//            Assert.That(firstLine, Does.Match(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}"),
//                "Log entry should contain a timestamp in the format yyyy-MM-dd HH:mm:ss.");
//        }

//        [Test]
//        public void Log_NullMessage_Should_Not_Throw()
//        {
//            string? nullMessage = null;

//            Assert.DoesNotThrow(() => LogUtil.Info(nullMessage), "Logging a null message should not throw an exception.");

//            Assert.That(File.Exists(_logFilePath), Is.True, "Log file should still be created.");
//            string content = File.ReadAllText(_logFilePath);

//            Assert.That(content, Does.Contain("[Info]"));
//        }

//        [Test]
//        public void Log_EmptyMessage_Should_Not_Throw()
//        {
//            string emptyMessage = string.Empty;

//            Assert.DoesNotThrow(() => LogUtil.Info(emptyMessage),
//                "Logging an empty string should not throw an exception.");

//            Assert.That(File.Exists(_logFilePath), Is.True, "Log file should be created.");
//            string content = File.ReadAllText(_logFilePath);
//            Assert.That(content, Does.Contain("[Info]"));
//        }
//    }
//}
