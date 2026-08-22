using GlobalKeyboardCapture.Maui.Configuration;
using GlobalKeyboardCapture.Maui.Core.Models;
using GlobalKeyboardCapture.Maui.Handlers;
using GlobalKeyboardCapture.Maui.Tests.TestDoubles;

namespace GlobalKeyboardCapture.Maui.Tests;

public sealed class BarcodeProfileTests
{
    [Fact]
    public void ProfileSupportsTabTerminatorAndStripsPrefixAndSuffix()
    {
        var options = new KeyHandlerOptions();
        var profile = new BarcodeScannerProfile("GS1")
        {
            MinLength = 3,
            Prefix = "]C1",
            Suffix = "!"
        };
        profile.TerminatorKeys.Clear();
        profile.TerminatorKeys.Add(KeyboardKey.Tab);
        options.BarcodeProfiles.Add(profile);
        var handler = new BarcodeHandler(options, new FakeTimeProvider());
        BarcodeScanResult? result = null;
        handler.ScanCompleted += (_, scan) => result = scan;

        handler.ShouldHandle(new KeyEventArgs { Key = KeyboardKey.Tab }).Should().BeTrue();

        Feed(handler, "]C1ABC!");
        var terminator = new KeyEventArgs { Key = KeyboardKey.Tab };
        handler.HandleKey(terminator);

        result.Should().NotBeNull();
        result!.Value.Should().Be("ABC");
        result.RawValue.Should().Be("]C1ABC!");
        result.ProfileName.Should().Be("GS1");
        result.PrefixRemoved.Should().BeTrue();
        result.SuffixRemoved.Should().BeTrue();
        terminator.Handled.Should().BeTrue();
    }

    [Fact]
    public void CompletedScanReportsTimingAndDeviceMetadata()
    {
        var time = new FakeTimeProvider();
        var options = new KeyHandlerOptions();
        options.BarcodeProfiles.Add(new BarcodeScannerProfile("device") { MinLength = 3 });
        var handler = new BarcodeHandler(options, time);
        var device = new KeyboardDeviceInfo(42, "Scanner");
        BarcodeScanResult? result = null;
        handler.ScanCompleted += (_, scan) => result = scan;

        foreach (var character in "ABC")
        {
            handler.HandleKey(new KeyEventArgs { Character = character, Device = device });
            time.Advance(TimeSpan.FromMilliseconds(10));
        }

        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter, Device = device });

        result.Should().NotBeNull();
        result!.Duration.Should().Be(TimeSpan.FromMilliseconds(20));
        result.CharacterCount.Should().Be(3);
        result.Device.Should().BeSameAs(device);
        result.TerminatorKey.Should().Be(KeyboardKey.Enter);
    }

    [Fact]
    public void ProfileSupportsControlCharacterTerminator()
    {
        var options = new KeyHandlerOptions();
        var profile = new BarcodeScannerProfile("STX-ETX") { MinLength = 3 };
        profile.TerminatorKeys.Clear();
        profile.TerminatorCharacters.Add('\u0003');
        options.BarcodeProfiles.Add(profile);
        var handler = new BarcodeHandler(options, new FakeTimeProvider());
        string? barcode = null;
        handler.BarcodeScanned += (_, value) => barcode = value;

        Feed(handler, "ABC");
        handler.HandleKey(new KeyEventArgs { Character = '\u0003' });

        barcode.Should().Be("ABC");
    }

    [Fact]
    public void RequiredPrefixRejectsUnqualifiedInput()
    {
        var options = new KeyHandlerOptions();
        options.BarcodeProfiles.Add(new BarcodeScannerProfile("qualified")
        {
            MinLength = 3,
            Prefix = "SCAN:",
            RequirePrefix = true
        });
        var handler = new BarcodeHandler(options, new FakeTimeProvider());
        var scans = 0;
        handler.ScanCompleted += (_, _) => scans++;

        Feed(handler, "ABC");
        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter });

        scans.Should().Be(0);
    }

    [Fact]
    public void AverageDelayCanRejectHumanTypingAndAcceptScannerSpeed()
    {
        var time = new FakeTimeProvider();
        var options = new KeyHandlerOptions();
        options.BarcodeProfiles.Add(new BarcodeScannerProfile("fast")
        {
            MinLength = 3,
            InterCharacterTimeout = TimeSpan.FromSeconds(1),
            MaxAverageInterCharacterDelay = TimeSpan.FromMilliseconds(20)
        });
        var handler = new BarcodeHandler(options, time);
        var scans = new List<string>();
        handler.BarcodeScanned += (_, value) => scans.Add(value);

        Feed(handler, "ABC", time, TimeSpan.FromMilliseconds(50));
        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter });
        Feed(handler, "XYZ", time, TimeSpan.FromMilliseconds(5));
        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter });

        scans.Should().Equal("XYZ");
    }

    [Fact]
    public void DeviceFilterRejectsOtherDevices()
    {
        var options = new KeyHandlerOptions();
        options.BarcodeProfiles.Add(new BarcodeScannerProfile("dedicated")
        {
            MinLength = 3,
            DeviceId = 7
        });
        var handler = new BarcodeHandler(options, new FakeTimeProvider());
        var scans = new List<string>();
        handler.BarcodeScanned += (_, value) => scans.Add(value);

        foreach (var character in "BAD")
            handler.HandleKey(new KeyEventArgs { Character = character, Device = new KeyboardDeviceInfo(8) });
        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter, Device = new KeyboardDeviceInfo(8) });

        foreach (var character in "GOOD")
            handler.HandleKey(new KeyEventArgs { Character = character, Device = new KeyboardDeviceInfo(7) });
        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter, Device = new KeyboardDeviceInfo(7) });

        scans.Should().Equal("GOOD");
    }

    [Fact]
    public void ProfileConfigurationIsSnapshottedAtConstruction()
    {
        var profile = new BarcodeScannerProfile("stable") { MinLength = 3 };
        var options = new KeyHandlerOptions();
        options.BarcodeProfiles.Add(profile);
        var handler = new BarcodeHandler(options, new FakeTimeProvider());
        string? scan = null;
        handler.BarcodeScanned += (_, value) => scan = value;

        profile.MinLength = 100;
        profile.TerminatorKeys.Clear();
        profile.TerminatorKeys.Add(KeyboardKey.Tab);

        Feed(handler, "ABC");
        handler.HandleKey(new KeyEventArgs { Key = KeyboardKey.Enter });

        scan.Should().Be("ABC");
    }

    [Fact]
    public void ProfileWithoutTerminatorIsRejected()
    {
        var profile = new BarcodeScannerProfile("invalid");
        profile.TerminatorKeys.Clear();
        var options = new KeyHandlerOptions();
        options.BarcodeProfiles.Add(profile);

        var act = () => new BarcodeHandler(options, new FakeTimeProvider());

        act.Should().Throw<ArgumentException>();
    }

    private static void Feed(
        BarcodeHandler handler,
        string value,
        FakeTimeProvider? timeProvider = null,
        TimeSpan? delay = null)
    {
        foreach (var character in value)
        {
            handler.HandleKey(new KeyEventArgs { Character = character });
            if (timeProvider is not null && delay.HasValue)
                timeProvider.Advance(delay.Value);
        }
    }
}
