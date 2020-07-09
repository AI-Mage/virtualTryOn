#import <Foundation/Foundation.h>
#import <MobileCoreServices/UTCoreTypes.h>
#import <ImageIO/ImageIO.h>
#import <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>

#ifdef UNITY_4_0 || UNITY_5_0
#import "iPhone_View.h"
#else
extern UIViewController* UnityGetGLViewController();
#endif

@interface UNativeCamera:NSObject
+ (int)checkPermission;
+ (int)requestPermission;
+ (int)canOpenSettings;
+ (void)openSettings;
+ (int)hasCamera;
+ (void)openCamera:(BOOL)imageMode defaultCamera:(int)defaultCamera savePath:(NSString *)imageSavePath maxImageSize:(int)maxImageSize videoQuality:(int)videoQuality maxVideoDuration:(int)maxVideoDuration;
+ (int)isCameraBusy;
+ (char *)getImageProperties:(NSString *)path;
+ (char *)getVideoProperties:(NSString *)path;
+ (char *)loadImageAtPath:(NSString *)path tempFilePath:(NSString *)tempFilePath maximumSize:(int)maximumSize;
@end

@implementation UNativeCamera

static NSString *pickedMediaSavePath;
static UIImagePickerController *imagePicker;
static int cameraMaxImageSize = -1;
static int imagePickerState = 0; // 0 -> none, 1 -> showing, 2 -> finished

// Credit: https://stackoverflow.com/a/20464727/2373034
+ (int)checkPermission {
	if ([[[UIDevice currentDevice] systemVersion] compare:@"7.0" options:NSNumericSearch] != NSOrderedAscending)
	{
		AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
		if (status == AVAuthorizationStatusAuthorized)
			return 1;
		else if (status == AVAuthorizationStatusNotDetermined )
			return 2;
		else
			return 0;
	}
	
	return 1;
}

// Credit: https://stackoverflow.com/a/20464727/2373034
+ (int)requestPermission {
	if ([[[UIDevice currentDevice] systemVersion] compare:@"7.0" options:NSNumericSearch] != NSOrderedAscending)
	{
		AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
		if (status == AVAuthorizationStatusAuthorized)
			return 1;
		
		if (status == AVAuthorizationStatusNotDetermined) {
			__block BOOL authorized = NO;
			
			dispatch_semaphore_t sema = dispatch_semaphore_create(0);
			[AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
				authorized = granted;
				dispatch_semaphore_signal(sema);
			}];
			dispatch_semaphore_wait(sema, DISPATCH_TIME_FOREVER);
			
			if (authorized)
				return 1;
			else
				return 0;
		}
			
		return 0;
	}
	
	return 1;
}

// Credit: https://stackoverflow.com/a/25453667/2373034
+ (int)canOpenSettings {
	if (&UIApplicationOpenSettingsURLString != NULL)
		return 1;
	else
		return 0;
}

// Credit: https://stackoverflow.com/a/25453667/2373034
+ (void)openSettings {
	if (&UIApplicationOpenSettingsURLString != NULL)
		[[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
}

+ (int)hasCamera {
	if ([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera])
		return 1;
	
	return 0;
}

// Credit: https://stackoverflow.com/a/10531752/2373034
+ (void)openCamera:(BOOL)imageMode defaultCamera:(int)defaultCamera savePath:(NSString *)imageSavePath maxImageSize:(int)maxImageSize videoQuality:(int)videoQuality maxVideoDuration:(int)maxVideoDuration {
	if (![UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera])
	{
		NSLog(@"Device has no registered cameras!");
		
		UnitySendMessage("NCCameraCallbackiOS", "OnMediaReceived", "");
		return;
	}
	
	if ((imageMode && ![[UIImagePickerController availableMediaTypesForSourceType:UIImagePickerControllerSourceTypeCamera] containsObject:(NSString*)kUTTypeImage]) ||
		(!imageMode && ![[UIImagePickerController availableMediaTypesForSourceType:UIImagePickerControllerSourceTypeCamera] containsObject:(NSString*)kUTTypeMovie]))
	{
		NSLog(@"Camera does not support this operation!");
		
		UnitySendMessage("NCCameraCallbackiOS", "OnMediaReceived", "");
		return;
	}
	
	imagePicker = [[UIImagePickerController alloc] init];
	imagePicker.delegate = self;
	imagePicker.allowsEditing = NO;
	imagePicker.sourceType = UIImagePickerControllerSourceTypeCamera;
	
	if (imageMode)
		imagePicker.mediaTypes = [NSArray arrayWithObject:(NSString *)kUTTypeImage];
	else
	{
		imagePicker.mediaTypes = [NSArray arrayWithObject:(NSString *)kUTTypeMovie];
		
		if (maxVideoDuration > 0)
			imagePicker.videoMaximumDuration = maxVideoDuration;
		
		if (videoQuality == 0)
			imagePicker.videoQuality = UIImagePickerControllerQualityTypeLow;
		else if (videoQuality == 1)
			imagePicker.videoQuality = UIImagePickerControllerQualityTypeMedium;
		else if (videoQuality == 2)
			imagePicker.videoQuality = UIImagePickerControllerQualityTypeHigh;
	}
	
	if (defaultCamera == 0 && [UIImagePickerController isCameraDeviceAvailable:UIImagePickerControllerCameraDeviceRear])
		imagePicker.cameraDevice = UIImagePickerControllerCameraDeviceRear;
	else if (defaultCamera == 1 && [UIImagePickerController isCameraDeviceAvailable:UIImagePickerControllerCameraDeviceFront])
		imagePicker.cameraDevice = UIImagePickerControllerCameraDeviceFront;
	
	pickedMediaSavePath = imageSavePath;
	cameraMaxImageSize = maxImageSize;
	
	imagePickerState = 1;
	[UnityGetGLViewController() presentViewController:imagePicker animated:YES completion:^{ imagePickerState = 0; }];
}

+ (int)isCameraBusy {
	if (imagePickerState == 2)
		return 1;
	
	if (imagePicker != nil) {
		if (imagePickerState == 1 || [imagePicker presentingViewController] == UnityGetGLViewController())
			return 1;
		
		imagePicker = nil;
		return 0;
	}
	
	return 0;
}

// Credit: https://stackoverflow.com/a/4170099/2373034
+ (NSArray *)getImageMetadata:(NSString *)path {
	int width = 0;
	int height = 0;
	int orientation = -1;

	CGImageSourceRef imageSource = CGImageSourceCreateWithURL((__bridge CFURLRef)[NSURL fileURLWithPath:path], nil);
	if (imageSource != nil) {
		NSDictionary *options = [NSDictionary dictionaryWithObject:[NSNumber numberWithBool:NO] forKey:(__bridge NSString *)kCGImageSourceShouldCache];
		CFDictionaryRef imageProperties = CGImageSourceCopyPropertiesAtIndex(imageSource, 0, (__bridge CFDictionaryRef)options);
		CFRelease(imageSource);

		CGFloat widthF = 0.0f, heightF = 0.0f;
		if (imageProperties != nil) {
			if (CFDictionaryContainsKey(imageProperties, kCGImagePropertyPixelWidth))
				CFNumberGetValue((CFNumberRef)CFDictionaryGetValue(imageProperties, kCGImagePropertyPixelWidth), kCFNumberCGFloatType, &widthF);
			
			if (CFDictionaryContainsKey(imageProperties, kCGImagePropertyPixelHeight))
				CFNumberGetValue((CFNumberRef)CFDictionaryGetValue(imageProperties, kCGImagePropertyPixelHeight), kCFNumberCGFloatType, &heightF);

			if (CFDictionaryContainsKey(imageProperties, kCGImagePropertyOrientation)) {
				CFNumberGetValue((CFNumberRef)CFDictionaryGetValue(imageProperties, kCGImagePropertyOrientation), kCFNumberIntType, &orientation);
				
				if (orientation > 4) { // landscape image
					CGFloat temp = widthF;
					widthF = heightF;
					heightF = temp;
				}
			}

			CFRelease(imageProperties);
		}

		width = (int)roundf(widthF);
		height = (int)roundf(heightF);
	}

	return [[NSArray alloc] initWithObjects:[NSNumber numberWithInt:width], [NSNumber numberWithInt:height], [NSNumber numberWithInt:orientation], nil];
}

+ (char *)getImageProperties:(NSString *)path {
	NSArray *metadata = [self getImageMetadata:path];
	
	int orientationUnity;
	int orientation = [metadata[2] intValue];
	
	// To understand the magic numbers, see ImageOrientation enum in NativeCamera.cs
	// and http://sylvana.net/jpegcrop/exif_orientation.html
	if (orientation == 1)
		orientationUnity = 0;
	else if (orientation == 2)
		orientationUnity = 4;
	else if (orientation == 3)
		orientationUnity = 2;
	else if (orientation == 4)
		orientationUnity = 6;
	else if (orientation == 5)
		orientationUnity = 5;
	else if (orientation == 6)
		orientationUnity = 1;
	else if (orientation == 7)
		orientationUnity = 7;
	else if (orientation == 8)
		orientationUnity = 3;
	else
		orientationUnity = -1;
	
	return [self getCString:[NSString stringWithFormat:@"%d>%d> >%d", [metadata[0] intValue], [metadata[1] intValue], orientationUnity]];
}

+ (char *)getVideoProperties:(NSString *)path {
	CGSize size = CGSizeZero;
	float rotation = 0;
	long long duration = 0;
	
	AVURLAsset *asset = [AVURLAsset URLAssetWithURL:[NSURL fileURLWithPath:path] options:nil];
	if (asset != nil) {
		duration = (long long) round(CMTimeGetSeconds([asset duration]) * 1000);
		CGAffineTransform transform = [asset preferredTransform];
		NSArray<AVAssetTrack *>* videoTracks = [asset tracksWithMediaType:AVMediaTypeVideo];
		if (videoTracks != nil && [videoTracks count] > 0) {
			size = [[videoTracks objectAtIndex:0] naturalSize];
			transform = [[videoTracks objectAtIndex:0] preferredTransform];
		}
		
		rotation = atan2(transform.b, transform.a) * (180.0 / M_PI);
	}
	
	return [self getCString:[NSString stringWithFormat:@"%d>%d>%lld>%f", (int)roundf(size.width), (int)roundf(size.height), duration, rotation]];
}

+ (UIImage *)scaleImage:(UIImage *)image maxSize:(int)maxSize {
	CGFloat width = image.size.width;
	CGFloat height = image.size.height;
	
	UIImageOrientation orientation = image.imageOrientation;
	if (width <= maxSize && height <= maxSize && orientation != UIImageOrientationDown &&
		orientation != UIImageOrientationLeft && orientation != UIImageOrientationRight &&
		orientation != UIImageOrientationLeftMirrored && orientation != UIImageOrientationRightMirrored &&
		orientation != UIImageOrientationUpMirrored && orientation != UIImageOrientationDownMirrored)
		return image;
	
	CGFloat scaleX = 1.0f;
	CGFloat scaleY = 1.0f;
	if (width > maxSize)
		scaleX = maxSize / width;
	if (height > maxSize)
		scaleY = maxSize / height;
	
	// Credit: https://github.com/mbcharbonneau/UIImage-Categories/blob/master/UIImage%2BAlpha.m
	CGImageAlphaInfo alpha = CGImageGetAlphaInfo(image.CGImage);
	BOOL hasAlpha = alpha == kCGImageAlphaFirst || alpha == kCGImageAlphaLast || alpha == kCGImageAlphaPremultipliedFirst || alpha == kCGImageAlphaPremultipliedLast;
	
	CGFloat scaleRatio = scaleX < scaleY ? scaleX : scaleY;
	CGRect imageRect = CGRectMake(0, 0, width * scaleRatio, height * scaleRatio);
	UIGraphicsBeginImageContextWithOptions(imageRect.size, !hasAlpha, image.scale);
	[image drawInRect:imageRect];
	image = UIGraphicsGetImageFromCurrentImageContext();
	UIGraphicsEndImageContext();
	
	return image;
}

+ (char *)loadImageAtPath:(NSString *)path tempFilePath:(NSString *)tempFilePath maximumSize:(int)maximumSize {
	// Check if the image can be loaded by Unity without requiring a conversion to PNG
	// Credit: https://stackoverflow.com/a/12048937/2373034
	NSString *extension = [path pathExtension];
	BOOL conversionNeeded = [extension caseInsensitiveCompare:@"jpg"] != NSOrderedSame && [extension caseInsensitiveCompare:@"jpeg"] != NSOrderedSame && [extension caseInsensitiveCompare:@"png"] != NSOrderedSame;

	if (!conversionNeeded) {
		// Check if the image needs to be processed at all
		NSArray *metadata = [self getImageMetadata:path];
		int orientationInt = [metadata[2] intValue];  // 1: correct orientation, [1,8]: valid orientation range
		if (orientationInt == 1 && [metadata[0] intValue] <= maximumSize && [metadata[1] intValue] <= maximumSize)
			return [self getCString:path];
	}
	
	UIImage *image = [UIImage imageWithContentsOfFile:path];
	if (image == nil)
		return [self getCString:path];
	
	UIImage *scaledImage = [self scaleImage:image maxSize:maximumSize];
	if (conversionNeeded || scaledImage != image) {
		if (![UIImagePNGRepresentation(scaledImage) writeToFile:tempFilePath atomically:YES]) {
			NSLog(@"Error creating scaled image");
			return [self getCString:path];
		}
		
		return [self getCString:tempFilePath];
	}
	else
		return [self getCString:path];
}

+ (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info {
	NSString *path = nil;
	if ([info[UIImagePickerControllerMediaType] isEqualToString:(NSString *)kUTTypeImage]) { // took picture
		// Temporarily save image as PNG
		UIImage *image = info[UIImagePickerControllerEditedImage] ?: info[UIImagePickerControllerOriginalImage];
		if (image == nil)
			path = nil;
		else {
			NSString *extension = [pickedMediaSavePath pathExtension];
			BOOL saveAsJPEG = [extension caseInsensitiveCompare:@"jpg"] == NSOrderedSame || [extension caseInsensitiveCompare:@"jpeg"] == NSOrderedSame;
			
			// Try to save the image with metadata
			// CANCELED: a number of users reported that this method results in 90-degree rotated images, uncomment at your own risk
			// Credit: https://stackoverflow.com/a/15858955
			/*NSDictionary *metadata = [info objectForKey:UIImagePickerControllerMediaMetadata];
			NSMutableDictionary *mutableMetadata = nil;
			CFDictionaryRef metadataRef;
			CFStringRef imageType;
			
			if (saveAsJPEG) {
				mutableMetadata = [metadata mutableCopy];
				[mutableMetadata setObject:@(1.0) forKey:(__bridge NSString *)kCGImageDestinationLossyCompressionQuality];
				
				metadataRef = (__bridge CFDictionaryRef)mutableMetadata;
				imageType = kUTTypeJPEG;
			}
			else {
				metadataRef = (__bridge CFDictionaryRef)metadata;
				imageType = kUTTypePNG;
			}
			
			CGImageDestinationRef imageDestination = CGImageDestinationCreateWithURL((__bridge CFURLRef)[NSURL fileURLWithPath:pickedMediaSavePath], imageType , 1, NULL);
			if (imageDestination == NULL )
				NSLog(@"Failed to create image destination");
			else {
				CGImageDestinationAddImage(imageDestination, image.CGImage, metadataRef);
				if (CGImageDestinationFinalize(imageDestination))
					path = pickedMediaSavePath;
				else
					NSLog(@"Failed to finalize the image");
				
				CFRelease(imageDestination);
			}*/
			
			if (path == nil) {
				//NSLog(@"Attempting to save the image without metadata as fallback");
				
				if ((saveAsJPEG && [UIImageJPEGRepresentation([self scaleImage:image maxSize:cameraMaxImageSize], 1.0) writeToFile:pickedMediaSavePath atomically:YES]) ||
					(!saveAsJPEG && [UIImagePNGRepresentation([self scaleImage:image maxSize:cameraMaxImageSize]) writeToFile:pickedMediaSavePath atomically:YES]) )
					path = pickedMediaSavePath;
				else {
					NSLog(@"Error saving image without metadata");
					path = nil;
				}
			}
		}
	}
	else { // recorded video
		NSURL *mediaUrl = info[UIImagePickerControllerMediaURL] ?: info[UIImagePickerControllerReferenceURL];
		if (mediaUrl == nil)
			path = nil;
		else
			path = [mediaUrl path];
	}

	imagePicker = nil;
	imagePickerState = 2;
	UnitySendMessage("NCCameraCallbackiOS", "OnMediaReceived", [self getCString:path]);

	[picker dismissViewControllerAnimated:NO completion:nil];
}

+ (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker
{
	imagePicker = nil;
	UnitySendMessage("NCCameraCallbackiOS", "OnMediaReceived", "");
	
	[picker dismissViewControllerAnimated:YES completion:nil];
}

// Credit: https://stackoverflow.com/a/37052118/2373034
+ (char *)getCString:(NSString *)source {
	if (source == nil)
		source = @"";
	
	const char *sourceUTF8 = [source UTF8String];
	char *result = (char*) malloc(strlen(sourceUTF8) + 1);
	strcpy(result, sourceUTF8);
	
	return result;
}

@end

extern "C" int _NativeCamera_CheckPermission() {
	return [UNativeCamera checkPermission];
}

extern "C" int _NativeCamera_RequestPermission() {
	return [UNativeCamera requestPermission];
}

extern "C" int _NativeCamera_CanOpenSettings() {
	return [UNativeCamera canOpenSettings];
}

extern "C" void _NativeCamera_OpenSettings() {
	[UNativeCamera openSettings];
}

extern "C" int _NativeCamera_HasCamera() {
	return [UNativeCamera hasCamera];
}

extern "C" void _NativeCamera_TakePicture(const char* imageSavePath, int maxSize, int preferredCamera) {
	[UNativeCamera openCamera:YES defaultCamera:preferredCamera savePath:[NSString stringWithUTF8String:imageSavePath] maxImageSize:maxSize videoQuality:-1 maxVideoDuration:-1];
}

extern "C" void _NativeCamera_RecordVideo(int quality, int maxDuration, int preferredCamera) {
	[UNativeCamera openCamera:NO defaultCamera:preferredCamera savePath:nil maxImageSize:4096 videoQuality:quality maxVideoDuration:maxDuration];
}

extern "C" int _NativeCamera_IsCameraBusy() {
	return [UNativeCamera isCameraBusy];
}

extern "C" char* _NativeCamera_GetImageProperties(const char* path) {
	return [UNativeCamera getImageProperties:[NSString stringWithUTF8String:path]];
}

extern "C" char* _NativeCamera_GetVideoProperties(const char* path) {
	return [UNativeCamera getVideoProperties:[NSString stringWithUTF8String:path]];
}

extern "C" char* _NativeCamera_LoadImageAtPath(const char* path, const char* temporaryFilePath, int maxSize) {
	return [UNativeCamera loadImageAtPath:[NSString stringWithUTF8String:path] tempFilePath:[NSString stringWithUTF8String:temporaryFilePath] maximumSize:maxSize];
}
