# upload-apk
Basic command line tool for uploading an APK to the Play Store.

Uses the [Google Play REST API](https://developers.google.com/android-publisher/) and currently only updates the alpha track.

```
UploadAndroidAPK

Required args: <application_id> <version> <file_path>
```

Requires OAuth2 access token from standard input so the output from [OAuth2TokenGenerator](https://github.com/manicmonkey/oauth2-token-generator) can be piped in.
