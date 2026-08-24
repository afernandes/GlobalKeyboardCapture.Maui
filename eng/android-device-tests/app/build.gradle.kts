plugins {
    id("com.android.application")
}

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
}

dependencies {
    androidTestImplementation("androidx.test.ext:junit:1.3.0")
    androidTestImplementation("androidx.test:runner:1.7.0")
    androidTestImplementation("androidx.test.uiautomator:uiautomator:2.3.0")
}
