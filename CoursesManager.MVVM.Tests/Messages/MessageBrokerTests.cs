using CoursesManager.MVVM.Messages;

namespace CoursesManager.MVVM.Tests.Messages;

internal class TestMessageOne : BaseMessage<TestMessageOne>
{
    public override TestMessageOne Clone()
    {
        return new TestMessageOne
        {
            MessageId = MessageId,
            TimeStamp = new(TimeStamp.Ticks)
        };
    }
}

internal class TestMessageTwo : BaseMessage<TestMessageTwo>
{
    public override TestMessageTwo Clone()
    {
        return new TestMessageTwo
        {
            MessageId = MessageId,
            TimeStamp = new(TimeStamp.Ticks)
        };
    }
}

public class MessageBrokerTests
{
    private IMessageBroker _messageBroker;

    [SetUp]
    public void SetUp()
    {
        _messageBroker = new MessageBroker();
    }

    [Test]
    public void Subscribe_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(null, this);
        });
    }

    [Test]
    public void Subscribe_ThrowsArgumentNullException_WhenRecipientIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ => { }, null);
        });
    }

    [Test]
    public void Publish_ThrowsArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _messageBroker.Publish<TestMessageOne>(null);
        });
    }

    [Test]
    public void Publish_InvokesOnlyTestMessageOne_WhenMessageIsProvided()
    {
        var shouldBeInvoked = false;
        var shouldNotBeInvoked = true;

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            shouldBeInvoked = true;
        }, this);

        _messageBroker.Subscribe<TestMessageTwo, MessageBrokerTests>(_ =>
        {
            shouldNotBeInvoked = false;
        }, this);

        _messageBroker.Publish(new TestMessageOne());

        Assert.That(shouldBeInvoked, Is.True);
        Assert.That(shouldNotBeInvoked, Is.True);
    }

    [Test]
    public void Publish_InvokesOnlyTestMessageTwo_WhenMessageIsProvided()
    {
        var shouldBeInvoked = false;
        var shouldNotBeInvoked = true;

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            shouldNotBeInvoked = false;
        }, this);

        _messageBroker.Subscribe<TestMessageTwo, MessageBrokerTests>(_ =>
        {
            shouldBeInvoked = true;
        }, this);

        _messageBroker.Publish(new TestMessageTwo());

        Assert.That(shouldBeInvoked, Is.True);
        Assert.That(shouldNotBeInvoked, Is.True);
    }

    [Test]
    public void Publish_SubscribersGetClonedMessage()
    {
        TestMessageTwo subscriberOneMessage = null;
        TestMessageTwo subscriberTwoMessage = null;

        _messageBroker.Subscribe<TestMessageTwo, MessageBrokerTests>(message =>
        {
            subscriberOneMessage = message;
        }, this);

        _messageBroker.Subscribe<TestMessageTwo, MessageBrokerTests>(message =>
        {
            subscriberTwoMessage = message;
        }, this);

        _messageBroker.Publish(new TestMessageTwo());

        Assert.That(ReferenceEquals(subscriberOneMessage, subscriberTwoMessage), Is.False);
    }

    [Test]
    public void Publish_ReturnsFalse_WhenNoSubscribers()
    {
        Assert.That(_messageBroker.Publish(new TestMessageOne()), Is.False);
    }

    [Test]
    public void Publish_ReturnsTrue_WhenThereAreSubscribers()
    {
        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ => { }, this);

        Assert.That(_messageBroker.Publish(new TestMessageOne()), Is.True);
    }

    [Test]
    public void Publish_AllSubscribersInvoked()
    {
        var shouldBeInvoked = true;
        var shouldAlsoBeInvoked = true;

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            shouldBeInvoked = true;
        }, this);

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            shouldAlsoBeInvoked = true;
        }, this);

        _messageBroker.Publish(new TestMessageOne());

        Assert.That(shouldBeInvoked, Is.True);
        Assert.That(shouldAlsoBeInvoked, Is.True);
    }

    [Test]
    public void Unsubscribe_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _messageBroker.Unsubscribe<TestMessageOne>(null);
        });
    }

    [Test]
    public void Unsubscribe_HandlerNotInvoked_WhenUnsubscribed()
    {
        var notInvoked = true;

        void Handler(TestMessageOne obj)
        {
            notInvoked = false;
        }

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(Handler, this);

        _messageBroker.Unsubscribe<TestMessageOne>(Handler);

        _messageBroker.Publish(new TestMessageOne());

        Assert.That(notInvoked, Is.True);
    }

    [Test]
    public void Unsubscribe_NotAllHandlersInvoked_WhenOneUnsubscribed()
    {
        var handlerOneNotInvoked = true;
        var handlerTwoInvoked = false;

        void HandlerOne(TestMessageOne obj)
        {
            handlerOneNotInvoked = false;
        }

        void HandlerTwo(TestMessageOne obj)
        {
            handlerTwoInvoked = true;
        }

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(HandlerOne, this);
        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(HandlerTwo, this);

        _messageBroker.Unsubscribe<TestMessageOne>(HandlerOne);

        _messageBroker.Publish(new TestMessageOne());

        Assert.That(handlerOneNotInvoked, Is.True);
        Assert.That(handlerTwoInvoked, Is.True);
    }

    [Test]
    public void Unsubscribe_DoesNotBreak_WhenNotRegisteredHandlerIsUnsubscribed()
    {
        void HandlerOne(TestMessageOne obj) { }

        Assert.DoesNotThrow(() =>
        {
            _messageBroker.Unsubscribe<TestMessageOne>(HandlerOne);
        });
    }

    [Test]
    public void Unsubscribe_DoesNotBreak_WhenHandlerWasNeverRegistered()
    {
        void HandlerOne(TestMessageOne obj) { }
        void HandlerTwo(TestMessageOne obj) { }

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(HandlerTwo, this);

        Assert.DoesNotThrow(() =>
        {
            _messageBroker.Unsubscribe<TestMessageOne>(HandlerOne);
        });
    }

    [Test]
    public void Unsubscribe_DoesNotRemove_WhenHandlerIsOfDifferentMessageType()
    {
        var handlerOneHit = false;
        var handlerTwoHit = false;

        void HandlerOne(TestMessageOne obj)
        {
            handlerOneHit = true;
        }

        void HandlerTwo(TestMessageTwo obj)
        {
            handlerTwoHit = true;
        }

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(HandlerOne, this);
        _messageBroker.Subscribe<TestMessageTwo, MessageBrokerTests>(HandlerTwo, this);

        _messageBroker.Unsubscribe<TestMessageOne>(HandlerOne);

        _messageBroker.Publish(new TestMessageOne());
        _messageBroker.Publish(new TestMessageTwo());

        Assert.That(handlerOneHit, Is.False);
        Assert.That(handlerTwoHit, Is.True);
    }

    [Test]
    public void UnsubscribeMe_ThrowsArgumentNullException_WhenNoRecipientProvided()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _messageBroker.UnsubscribeMe<MessageBrokerTests>(null);
        });
    }

    [Test]
    public void UnsubscribeMe_NoHandlersInvoked_WhenUnsubscribed()
    {
        var handlerOneNotInvoked = true;
        var handlerTwoNotInvoked = true;

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            handlerOneNotInvoked = false;
        }, this);

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            handlerTwoNotInvoked = false;
        }, this);

        _messageBroker.UnsubscribeMe(this);

        _messageBroker.Publish(new TestMessageOne());

        Assert.That(handlerOneNotInvoked, Is.True);
        Assert.That(handlerTwoNotInvoked, Is.True);
    }

    [Test]
    public async Task Publish_Concurrency_WhenManyPublishesAreMade()
    {
        var message = new TestMessageOne();

        int received = 0;
        int totalSubscribers = 1000;
        int amountOfPublishes = 100;
        int expectedReceived = totalSubscribers * amountOfPublishes;

        Parallel.For(0, totalSubscribers, _ =>
        {
            _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ => Interlocked.Increment(ref received), this);
        });

        var tasks = new List<Task>();
        for (int i = 0; i < amountOfPublishes; i++)
        {
            tasks.Add(Task.Run(() => _messageBroker.Publish(message)));
        }

        await Task.WhenAll(tasks);

        Assert.That(received, Is.EqualTo(expectedReceived));
    }

    [Test]
    public void UnsubscribeAll_DoesAsStated()
    {
        var handlerOneNotInvoked = true;
        var handlerTwoNotInvoked = true;

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            handlerOneNotInvoked = false;
        }, this);

        _messageBroker.Subscribe<TestMessageOne, MessageBrokerTests>(_ =>
        {
            handlerTwoNotInvoked = false;
        }, this);

        _messageBroker.UnsubscribeAll();

        _messageBroker.Publish(new TestMessageOne());

        Assert.That(handlerOneNotInvoked, Is.True);
        Assert.That(handlerTwoNotInvoked, Is.True);
    }
}