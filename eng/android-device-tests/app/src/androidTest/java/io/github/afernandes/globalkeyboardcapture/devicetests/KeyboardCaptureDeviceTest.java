package io.github.afernandes.globalkeyboardcapture.devicetests;

import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

import android.app.Instrumentation;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.view.KeyEvent;

import androidx.test.ext.junit.runners.AndroidJUnit4;
import androidx.test.platform.app.InstrumentationRegistry;
import androidx.test.uiautomator.By;
import androidx.test.uiautomator.Direction;
import androidx.test.uiautomator.UiDevice;
import androidx.test.uiautomator.UiObject2;
import androidx.test.uiautomator.Until;

import org.junit.Test;
import org.junit.runner.RunWith;

@RunWith(AndroidJUnit4.class)
public final class KeyboardCaptureDeviceTest {
    private static final String TAG = "GKC.DeviceTest";
    private static final String TARGET_PACKAGE =
        "com.companyname.globalkeyboardcapturemauisample";
    private static final long TIMEOUT_MILLISECONDS = 10_000;

    @Test
    public void capturesFunctionNumpadAndScannerInput() {
        Instrumentation instrumentation = InstrumentationRegistry.getInstrumentation();
        UiDevice device = UiDevice.getInstance(instrumentation);
        Context context = instrumentation.getContext();
        launchSample(context, device);

        pressAndExpect(device, KeyEvent.KEYCODE_F1, "F1");
        pressAndExpect(device, KeyEvent.KEYCODE_F12, "F12");

        assertTrue("Numpad Enter could not be injected.",
            device.pressKeyCode(KeyEvent.KEYCODE_NUMPAD_ENTER));
        waitForText(device, "Enter");
        waitForTextContaining(device, "native=160");
        waitForTextContaining(device, "location=Numpad");
        Log.i(TAG, "PASS NumpadEnter");

        int[] barcodeKeys = new int[] {
            KeyEvent.KEYCODE_1,
            KeyEvent.KEYCODE_2,
            KeyEvent.KEYCODE_3,
            KeyEvent.KEYCODE_4,
            KeyEvent.KEYCODE_5,
            KeyEvent.KEYCODE_ENTER
        };
        assertTrue("Barcode key sequence could not be injected.",
            device.pressKeyCodes(barcodeKeys));
        scrollAndWaitForText(device, "12345");
        Log.i(TAG, "PASS KeyboardWedgeScanner");
    }

    private static void launchSample(Context context, UiDevice device) {
        Intent intent = context.getPackageManager().getLaunchIntentForPackage(TARGET_PACKAGE);
        assertNotNull("The MAUI sample is not installed.", intent);
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK | Intent.FLAG_ACTIVITY_NEW_TASK);
        context.startActivity(intent);

        assertTrue("The MAUI sample did not reach the foreground.",
            device.wait(Until.hasObject(By.pkg(TARGET_PACKAGE).depth(0)),
                TIMEOUT_MILLISECONDS));
        Log.i(TAG, "PASS SampleLaunched");
    }

    private static void pressAndExpect(UiDevice device, int keyCode, String text) {
        assertTrue("Key code " + keyCode + " could not be injected.",
            device.pressKeyCode(keyCode));
        waitForText(device, text);
        Log.i(TAG, "PASS " + text);
    }

    private static void waitForText(UiDevice device, String text) {
        assertTrue("Text was not rendered: " + text,
            device.wait(Until.hasObject(By.text(text)), TIMEOUT_MILLISECONDS));
    }

    private static void waitForTextContaining(UiDevice device, String text) {
        assertTrue("Text fragment was not rendered: " + text,
            device.wait(Until.hasObject(By.textContains(text)), TIMEOUT_MILLISECONDS));
    }

    private static void scrollAndWaitForText(UiDevice device, String text) {
        if (device.hasObject(By.text(text)))
            return;

        UiObject2 scrollable = device.findObject(By.scrollable(true));
        if (scrollable != null)
            scrollable.scroll(Direction.DOWN, 1.0f);
        waitForText(device, text);
    }
}
