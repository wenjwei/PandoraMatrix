@echo ChannelSpliter kingchild@163.com
@echo %4
@echo %4
@echo %3
@echo %4

@set pngPath=%1
@set rgbPath=%2
@set alphaPath=%3
@set toolPath=%4

cd /d %toolPath%
convert %pngPath% -channel Alpha -separate %alphaPath%
convert %pngPath% -channel Alpha -threshold 100%% +channel %rgbPath%
@echo Life is short, coding hard.
