Welcome to Flurry Analytics!

This archive should contain these files:

 - FlurryWP7SDK.dll
   The library containing Flurry's collection and reporting code.
   
 - ProjectApiKey.txt
   This file contains the name of your project and your project's API key.

 - README.txt
   This text file containing instructions.  
 
To integrate Flurry Analytics into your Windows Phone 7 application:

1. Reference FlurryWP7SDK.dll in your application project.
   
2. Configure WMAppManifest.xml:

   Required Capability:
   - ID_CAP_NETWORKING
     is required to send analytics data back to the Flurry servers.
   
   - ID_CAP_IDENTITY_USER
     is required to retrieve ANID for reporting. 
     
   - ID_CAP_IDENTITY_DEVICE
	 is required to retrieve Device model and firmware.

   Specify Version attribute in the manifest to have data reported under that version.
  
3. Add lifecycle calls

  - Insert a call to FlurryWP7SDK.Api.StartSession(string apiKey) to begin a session,
    passing it your project's API key. It is recommended to insert this call after
    the application is launched or reactivated. If you don't intend to explicitly end
    the session before application closes, this is all you need to do.
    
  - FlurryWP7SDK.Api.EndSession() is already associated with the Closing function,
    and FlurryWP7SDK.Api.PauseSession() is already associated with the Deactivated function,
    so there is no need to make explicit calls to these functions in your application
    unless you need to call EndSession explicitly before the application terminates.
    
   - If you have an active session, make sure StartSession is called when the application is
     reactivated to resume the current session.  Session length, usage frequency, events and
     errors will continue to be tracked as part of the same session.  If the application is
     reactivated after more than 10 seconds of deactivation, the current session will be ended
     and a new one will be started.  If you wish to change the window during which a session
     can be resumed, call FlurryWP7SDK.Api.SetSessionContinueSeconds(int seconds) after the
     session has started.    
   
   - If you want to track Activity usage, we recommend using LogEvent, described below.     
    
4. Build your application normally.

That's all you need to do to begin receiving basic metric data.  You can use the following
functions to report additional data:

   - FlurryWP7SDK.Api.LogEvent(string eventId, bool timed, List<Parameter> parameters)
     Use LogEvent to track user events that happen during a session.  You can track how many
     times each event occurs, what order events happen in, as well as what the most common
     parameters are for each event.  This can be useful for measuring how often users take
     various actions, or what sequences of actions they usually perform. Each project
     supports a maximum of 100 events.  The timed argument and the parameters argument are
     both optional. Each event id, parameter key, and parameter value must be no more than
     255 characters in length.  Each event can have no more than 10 parameters. If the
     timed argument is true, that means you are logging a timed event, which can be ended by
     calling the FlurryWP7SDK.Api.EndTimedEvent(string eventId, List<Parameter> parameters)
     function using the same eventid used to start the timed event.

   - FlurryWP7SDK.Api.LogError(String message, Exception exception)
     Use LogError to report application errors.  Flurry will report the last 10 errors to
     occur in each session. (max length 255 chars)

Optional configuration methods:

  Call these methods after calling StartSession to change the configuration:

   - FlurryWP7SDK.Api.SetVersion(string versionName)
     To change the version name your analytic data is reported under.
     If this is not specified, the version name is retrieved from the application descriptor.
   
   - FlurryWP7SDK.Api.SetUserId(string userId)
     FlurryWP7SDK.Api.SetAge(int age)
     FlurryWP7SDK.Api.SetGender(Gender gender)
     FlurryWP7SDK.Api.SetLocation(double latitude, double longitude, float accuracy)
     To record demographic data about the user.
       
   - FlurryWP7SDK.Api.SetSessionContinueSeconds(int seconds)
     Pass a value to change the number of seconds for which paused sessions will be
     continued. After this amount of time has passed with no activity, a new session
     is assumed to have started.
     
   - FlurryWP7SDK.Api.SetSecureTransportEnabled()
     To pass data over SSL. This should be called before the call to StartSession.  
     
  - FlurryWP7SDK.Api.SetReportDelay(int seconds)
    If you want to delay the reporting of data from the beginning of a session, you can
    use this method to have the agent delay its report for this long.

Please let us know if you have any questions. If you need any help, just email winmosupport@flurry.com!

Cheers,
The Flurry Team
http://www.flurry.com
winmosupport@flurry.com

 