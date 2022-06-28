-- Mac Unity premake module
local platform = {}

function platform.setupTargetAndLibDir(baseTargetDir)
    targetdir(baseTargetDir .. "Mac/%{cfg.buildcfg}")
    libdirs("${WWISESDK}/Mac/%{cfg.buildcfg}/lib")
end

function platform.platformSpecificConfiguration()
    links {
        "AudioToolbox.framework",
        "AudioUnit.framework",
        "Foundation.framework",
        "CoreAudio.framework",
        "AkAACDecoder"
    }
	
	xcodebuildsettings {
		GENERATE_INFOPLIST_FILE = "YES"
	}
end

return platform