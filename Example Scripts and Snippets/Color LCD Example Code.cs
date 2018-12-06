string border = "-";    
string space = " ";    
string chart = "";    
    
int line = 52;    
     
     
void Main(string command)          
{   
 
//Echo(command); 
   
    // Example: LCDText|Clean or Append|YOUR-LCD-NAME|Title*THIS IS A TEST TITLE|BR|Blank|BR|Text*Test Chart|Chart*Loading Clang..*50.0|CheckList*Autorun:1#Docked:1#Full:0   
     
    string[] list = command.Split('|');  
   
    IMyTextPanel panel = GridTerminalSystem.GetBlockWithName(list[0]) as IMyTextPanel; 
 
     if(panel == null)       
     {       
        // This will give you an error with a message.  
        Echo("Error: "+list[0]+" Not found, please check argument.");      
 
     }else{ 
 
    LCDSetup(panel); 
   
    StringBuilder sb = new StringBuilder(); 
   
    int count = list.Length; 
   
    for(int i = 2; i < count; i++)   
    {   
        string[] txt = list[i].Split('*');   
 
   
             switch (txt[0]) {   
                case "Title": 
                    sb.Append(Title(txt[1]));   
                    break; 
                case "Alert": 
                    sb.Append(Alert(txt[1],txt[2],txt[3])); 
                break; 
                case "CheckList": 
                    sb.Append(CheckList(txt[1])); 
                break;   
                case "Text":  
                    sb.Append(Text(txt[1]));       
                    break;   
                case "TextC": 
                    sb.Append(TextC(txt[1]));       
                    break;   
                case "TextR":  
                    sb.Append(TextR(txt[1]));       
                    break;   
                case "Chart":    
                    double j = Convert.ToDouble(txt[2]);   
                    sb.Append(Chart(txt[1],j));       
                    break;   
                case "Blank":    
                    sb.Append(Blank());       
                    break;   
                case "HR":    
                    sb.Append(HR());       
                    break;   
                default:       
                    Echo("Command not found: " + txt[0]);       
                    break;       
                }     
   
   
    }   
 
    if(list[1] == "Clear") 
    {     
        panel.WritePublicText(sb.ToString(), false);    
 
    }else 
    { 
        string prefix = panel.GetPublicText(); 
        panel.WritePublicText(prefix+sb.ToString(), false);  
    } 
} 
 
} 
 
 
void LCDSetup(IMyTextPanel panel) 
{ 
 
    const long Monospace = 1147350002; 
    Color BackgroundColor = new Color(0,0,0); 
    Color FontColor = new Color(255,255,255); 
 
     
    panel.SetValue<long>("Font", Monospace); 
    panel.SetValue("FontSize", 0.5f); 
    panel.SetValue("FontColor", FontColor); 
    panel.SetValue("BackgroundColor", BackgroundColor); 
 
    panel.ShowTextureOnScreen(); 
    panel.ShowPublicTextOnScreen(); 
 
 
} 
 
string Alert(string type, string title, string message = "") 
{ 
 
    StringBuilder sb = new StringBuilder(); 
 
    string success = ""; 
    string successicon = ""; 
    string warning = ""; 
    string warningicon = ""; 
    string info = ""; 
    string infoicon = ""; 
    string error = ""; 
    string erroricon = ""; 
    string alerttype = ""; 
    string alerticon = ""; 
 
     
 
    switch (type) { 
                case "Success": 
                    alerttype = success; 
                    alerticon = successicon; 
                    break; 
                case "Warning": 
                    alerttype = warning; 
                    alerticon = warningicon;    
                    break; 
                case "Info": 
                    alerttype = info; 
                    alerticon = infoicon;   
                    break; 
                case "Error": 
                    alerttype = error; 
                    alerticon = erroricon;     
                    break; 
                default:     
                    Echo("Alert type not found, please check argument.");     
                    break;     
                } 
 
 
    if(alerttype != "" && alerticon != "") 
    { 
 
        string header = alerticon+" "+title; 
 
        int len = header.Length;   
        int half = ((line-len)/2); 
 
        //Top row 
        for(int i = 1; i<= line;i++){ 
 
            sb.Append(alerttype);    
 
        }  
        sb.Append("\n"); 
 
        //Blank row 
        sb.Append(alerttype); 
        for(int i = 1; i<= (line-2);i++){ 
 
            sb.Append(space);    
 
        } 
        sb.Append(alerttype); 
        sb.Append("\n"); 
 
        //Title row 
        sb.Append(alerttype); 
        for(int i = 1; i <= half;i++)   
        {     
            sb.Append(space);   
        }    
 
            sb.Append(header); 
 
        for(int i = 1; i <= (half-1);i++)   
        {     
            sb.Append(space);   
        } 
        sb.Append(alerttype); 
        sb.Append("\n"); 
 
        //Blank row 
        sb.Append(alerttype); 
        for(int i = 1; i<= (line-2);i++){ 
 
            sb.Append(space);    
 
        } 
        sb.Append(alerttype); 
        sb.Append("\n"); 
        //Bottom row 
        for(int i = 1; i<= line;i++){ 
 
            sb.Append(alerttype);    
 
        }  
        sb.Append("\n"); 
        sb.Append("\n"); 
        sb.Append("\n"); 
 
        len = message.Length;   
        half = ((line-len)/2); 
 
        //Message row 
        for(int i = 1; i <= half;i++)   
        {     
            sb.Append(space);   
        }    
 
            sb.Append(message); 
 
        for(int i = 1; i <= half;i++)   
        {     
            sb.Append(space);   
        } 
        sb.Append("\n"); 
 
         
 
    }   
     
    return sb.ToString(); 
 
} 
     
string Title(string title)   
{     
    StringBuilder sb = new StringBuilder();   
     
    int len = title.Length;   
    int half = ((line-len)/2);   
    
    sb.Append(HR());   
    sb.Append("\n");   
    for(int i = 1; i <= half;i++)   
    {     
       sb.Append(space);   
    }     
        sb.Append(title);   
    for(int i = 1; i <= half;i++)   
    {     
       sb.Append(space);   
    }     
     
    sb.Append("\n");   
    sb.Append("\n");   
    sb.Append(HR());   
     
    return sb.ToString();   
     
}  
 
string CheckList(string list) 
{ 
 
    StringBuilder sb = new StringBuilder(); 
 
    //Autorun:1#Docked:1#Full:0 
 
    string left = "["; 
    string right = "]"; 
    string t = "■"; 
 
    string[] boxes = list.Split('#'); 
 
    int count = boxes.Length;   
   
    for(int i = 0; i < count; i++) 
    {    
         
        string[] val = boxes[i].Split(':'); 
        Echo(val[0]); 
        sb.Append("          "+left); 
 
        if(val[1] == "1"){ 
            sb.Append(t); 
        }else{ 
            sb.Append(" "); 
        } 
         
        sb.Append(right+" "+val[0]); 
        sb.Append("\n"); 
 
    } 
 
    return sb.ToString(); 
 
 
 
}  
   
string Logo()   
{   
    StringBuilder sb = new StringBuilder();   
    sb.Append("   __ _________   ____        __         __           \n");   
    sb.Append("  / //_/  _/ _ | /  _/__  ___/ /_ _____ / /_______ __ \n");   
    sb.Append(" / ,< _/ // __ |_/ // _ \\/ _  / // (_-</ __/ __/ // / \n");   
    sb.Append("/_/|_/___/_/ |_/___/_//_/\\_,_/\\_,_/___/\\__/_/  \\_, /  \n");   
    sb.Append("                                              /___/   \n");   
   
   
    return sb.ToString();   
   
   
}   
   
string Text(string str)   
{   
   
    return str+"\n";   
   
}   
   
string TextR(string str)   
{   
   
    StringBuilder sb = new StringBuilder();   
    int len = str.Length;   
    int end = (line-len);   
   
    for(int i = 1; i <= end;i++)   
    {   
       sb.Append(space);   
    }   
        sb.Append(str+"\n");   
   
    return sb.ToString();   
   
}   
   
string TextC(string str)   
{   
   
    StringBuilder sb = new StringBuilder();   
    int len = str.Length;   
    int half = ((line-len)/2);   
   
    for(int i = 1; i <= half;i++)   
    {   
       sb.Append(space);   
    }   
        sb.Append(str);   
    for(int i = 1; i <= half;i++)   
    {   
       sb.Append(space);   
    }   
    sb.Append("\n");   
   
    return sb.ToString();   
   
}   
    
string Chart(string title,double per)    
{    
    
    string topleft = "╒";    
    string topright = "╕";    
    string side = "│";    
    string bottomleft = "╘";    
    string bottomright = "╛";    
    string hor = "═"; 
 
    string red = ""; 
    string yellow = ""; 
    string green = "";    
    
    StringBuilder sb = new StringBuilder();    
    
   // Title    
    int len = title.Length;     
    int half = ((line-len)/2);     
    
    sb.Append("\n");     
    for(int i = 1; i <= half;i++)     
    {     
       sb.Append(space);     
    }     
        sb.Append(title);     
    for(int i = 1; i <= half;i++)     
    {     
       sb.Append(space);     
    }     
    sb.Append("\n");     
        
   // Chart Top    
    sb.Append(topleft);    
    
    for(int i = 1; i <= (line-1);i++)    
    {    
        sb.Append(hor);     
    }    
    
    sb.Append(topright);           
    sb.Append("\n"); 
    
        double li = 50.0;   
        int cal = Convert.ToInt32(Math.Round(((li/100.0)*per), MidpointRounding.ToEven));    
        int spaces = Convert.ToInt32((li-cal));    
 
            sb.Append(side);     
               
            if(cal != 0)   
            {   
                int t = 0; 
                for(int i = 0; i <= cal; i++)    
                {    
 
                    if(t < 16) 
                    { 
                        chart = red; 
                    }else if(t >= 16 && t < 35){ 
 
                        chart = yellow; 
 
                    }else{ 
 
                        chart = green; 
                    } 
 
                    sb.Append(chart); 
                    t++;    
                }    
            }   
    
            for(int i = 0; i < spaces; i++)    
            {    
                sb.Append(space);    
            }    
    
    
            sb.Append(side);     
            sb.Append("\n");    
   
      // Chart Bottom    
        sb.Append(bottomleft);    
       
        for(int i = 1; i <= (line-1);i++)    
        {    
            sb.Append(hor);     
        }    
       
        sb.Append(bottomright);     
           
        sb.Append("\n");    
    
    return sb.ToString();     
    
}    
     
string HR()     
{     
 
    StringBuilder sb = new StringBuilder(); 
 
    sb.Append("\n");  
    for(int i = 1; i <= line;i++)    
        {    
            sb.Append(border);     
        } 
    sb.Append("\n");   
 
     
    return sb.ToString();     
     
}    
    
string Blank()     
{     
     
    return "\n";     
     
}