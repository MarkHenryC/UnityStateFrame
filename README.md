# UnityStateFrame
__State machine framework for activity-based games.__
It was originally built for dual Oculus Go and desktop PC games. Go code is now stripped out.
* Single event class that can be fed with any input data
* Tracks picking and other activities like pouring, blowing, shooting etc
* Simple way of overriding default behaviour based on current activity and state
* Create a new activity (sub-game) with default activity script or custom one from tools menu
* Create states within current activity from script menu & override behaviours as required
* Easy way of managing assets that can be specific to state, activity, global or shared
* Order of activities and states can be shuffled with no code mods required
