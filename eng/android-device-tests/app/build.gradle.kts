plugins {
    id("com.android.application")
}

val integrationKeystorePath = System.getenv("GKC_ANDROID_KEYSTORE")

android {
    namespace = "io.github.afernandes.globalkeyboardcapture.devicetests"
    compileSdk = 36

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    defaultConfig {
        // The test APK targets the MAUI sample installed by CI/Firebase. Building
        // this placeholder app is not part of the package or release artifacts.
        applicationId = "com.companyname.globalkeyboardcapturemauisample"
        minSdk = 21
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"
        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    if (!integrationKeystorePath.isNullOrBlank()) {
        val integrationSigning = signingConfigs.create("integration") {
            storeFile = file(integrationKeystorePath)
            storePassword = "android"
            keyAlias = "androiddebugkey"
            keyPassword = "android"
        }
        buildTypes.getByName("debug").signingConfig = integrationSigning
    }
}

dependencies {
    androidTestImplementation("androidx.test.ext:junit:1.3.0")
    androidTestImplementation("androidx.test:runner:1.7.0")
    androidTestImplementation("androidx.test.uiautomator:uiautomator:2.3.0")

    // AndroidX Test Core still requests lifecycle-common 2.3.1. The MAUI 10
    // target APK ships Lifecycle 2.9.2; instrumentation shares one process and
    // an older Lifecycle.Event in the test APK crashes EmojiCompat startup.
    androidTestImplementation("androidx.lifecycle:lifecycle-common:2.9.2")
}
