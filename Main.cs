// You need a timer set to TRIGGER itself and call this script.
// Paste your rom here AFTER converting it at http://jpillora.com/base64-encoder
// DONT PASTE THE ROM IN GAME, COPY THE SCRIPT TO A TEXT EDITOR FIRST. 
//   IT WILL FREEZE THE GAME IF YOU EVEN CLICK ON THE FOLLOWING LINE IN GAME!
string rom="";
//rom source: https://github.com/Skillath/GameBoyFlappyBird

// Adjust the throttle/frameSkips to eliminate complexity errors.
static int throttle = 1000;
static int frameSkips = 4;
static string lcdName = "SpaceGameboy LCD";
static int tapLength = 4;

int stage = 0;
SpaceGameboy gb;
Dictionary<string, bool> tgls = new Dictionary<string, bool>() {
    {"up", false},
    {"down", false},
    {"right", false},
    {"left", false},
    {"a", false},
    {"b", false},
    {"start", false},
    {"select", false}
};

Dictionary<string, int> taps = new Dictionary<string, int>() {
    {"up", 0},
    {"down", 0},
    {"right", 0},
    {"left", 0},
    {"a", 0},
    {"b", 0},
    {"start", 0},
    {"select", 0}
};

Dictionary<string, bool> tgl = new Dictionary<string, bool>() {
    {"up", false},
    {"down", false},
    {"right", false},
    {"left", false},
};

public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main(string arg)
{
    switch(stage)
    {
        case 0:
            if(arg == "storage" || rom == "") rom = Storage;
            var lcd = GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            if(lcd != null)
            {
                gb = new SpaceGameboy(lcd,Echo);
            }
            else
            {
                throw new Exception(lcdName + " not found!");
            }
            break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
        case 6:
            gb.reset(rom, stage);
            break;
        default:
            foreach(var b in new string[]{"up", "down", "left", "right", "a", "b", "start", "select"})
            {
                if(taps[b] > 0)
                {
                    taps[b]--;
                    if(taps[b] == 0) gb.keyup(b);
                }
            }
            if(arg.EndsWith("On")) gb.keydown(arg.Remove(arg.Length - 2));
            else if(arg.EndsWith("Off")) gb.keyup(arg.Remove(arg.Length - 3));
            else if(arg.EndsWith("Toggle"))
            {
                string btn = arg.Remove(arg.Length - 6);
                if(tgls[btn])
                {
                    gb.keyup(btn);
                    tgls[btn] = false;
                }
                else
                {
                    gb.keydown(btn);
                    tgls[btn] = true;                        
                }
            }
            else if(arg.EndsWith("Arrow"))
            {
                string btn = arg.Remove(arg.Length - 5);
                gb.keydown(btn);
                foreach(var b in new string[]{"up", "down", "right", "left"})
                {
                    if(tgl[b])
                    {
                        gb.keyup(b);
                        tgl[b] = false;
                    }
                    else if(btn == b)
                    {
                        tgl[btn] = true;        
                    }
                }                    
            }
            else
            {
                gb.keydown(arg);
                taps[arg] = tapLength;
            }
            gb.frame(throttle, frameSkips, stage);
            if((stage % 2) == 0) gb.update();
            break;
    }
    stage++;
}