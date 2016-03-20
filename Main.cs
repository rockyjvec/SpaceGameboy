SpaceGameboy gb;

void Main(string argument)
{
    if(gb == null)
    {
        var lcd = GridTerminalSystem.GetBlockWithName("SpaceGameboy LCD") as IMyTextPanel;
        
        if(lcd != null)
        {
            gb = new SpaceGameboy(lcd);
            
            gb.reset(lcd.GetPrivateText());
        }
        else
        {
            throw new Exception("SpaceGameboy LCD not found!");
        }        
    }

    switch(argument)
    {
        case "up":
            break;
    }
    gb.frame();
}
