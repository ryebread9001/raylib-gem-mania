using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.KeyboardKey;
using static Raylib_cs.MouseButton;
using static Raylib_cs.Color;


const int screenWidth = 800;
const int screenHeight = 450;
int mult = 1;
int gamestate = 1;
int speed = 1;
int limit = 100;

InitWindow(screenWidth, screenHeight, "raylib-project");
SetTargetFPS(60);
InitAudioDevice();      // Initialize audio device

Sound fxWav = LoadSound("sound1.wav");         // Load WAV audio file
Sound fx2Wav = LoadSound("sound2.wav");        // Load OGG audio file
Sound fx3Wav = LoadSound("hitHurt.wav");
SetSoundVolume(fxWav, 0.2f);

Player player = new Player(400, 400);
Asteroid[] asteroids = new Asteroid[55];
Gem[] gems = new Gem[10];
Random rnd = new Random();
Health hlth = new Health(10);
Score scr = new Score(0);
Collision col = new Collision();
MainScreen mainscreen = new MainScreen();


for (int i = 0; i < 10; i++)
{
    Gem gm = new Gem(0, 0);
    gems[i] = gm;
    gems[i].Spawn();
}

for (int j = 0; j < 55; j++)
{
    Asteroid ast = new Asteroid(0, 0);
    asteroids[j] = ast;//returns random integers < 10
    asteroids[j].Spawn();
}

while (!WindowShouldClose())    // Detect window close button or ESC key
{
    
    if (gamestate == 2) { //gamestate 2 is our gameplay screen
        if (IsKeyDown(KEY_RIGHT)) //keyboard input
            player.MoveRight();
        else if (IsKeyDown(KEY_LEFT))
            player.MoveLeft();
        foreach (Asteroid ast in asteroids) { // looping throug arrays of fallinf objects to update them
            ast.Fall(speed);
        }
        foreach (Gem gm in gems) {
            gm.Fall(speed);
        }
        if (scr.score > limit) {
            speed += 1;
            limit += limit*2;
            PlaySoundMulti(fx2Wav); // sound fx woo woo
        }
        BeginDrawing();
        ClearBackground(BLACK);
        player.DrawPlayer(); // player, health, scr drawing
        hlth.DrawHealth();
        scr.DrawScore();
        if (hlth.health <= 0) // death logic
        {
            gamestate = 1;
            //
            hlth.health = 10;
            speed = 2;
            mult = 1;
            using(StreamWriter writetext = new StreamWriter("HighScores.txt", true)) // write score to txt file
            {
                writetext.Write($"{scr.score}\n");
            }
        }
        foreach (Asteroid ast in asteroids) { // drawing asteroids
            ast.DrawAsteroid();
            
            if (col.CheckCollisionAsteroid(player, ast)) { // asteroid collision check call and collision logic
                Console.WriteLine("COLLISION");
                ast.Spawn();
                hlth.TakeDamage();
                scr.Set(1);
                mult = 1;
                PlaySoundMulti(fx3Wav);
            }
        }
        foreach (Gem gm in gems) {
            gm.DrawGem();

            if (col.CheckCollisionGem(player, gm)) { // gem collision check call and logic
                Console.WriteLine("GEM");
                gm.Spawn();
                scr.Set(mult);
                scr.Increase(mult);
                mult++;
                PlaySoundMulti(fxWav); //fx woo woo
            }
        }
        EndDrawing();
        //WaitTime(0.01);
    } else {
        mainscreen.Draw(scr.score); // gamestate 1 is home screen
        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON))
        {
            //if (hlth.health == 0) hlth.health+=10;
            gamestate = 2;
            scr.score = 0; // reset values
        } 
    }
    
}
// class which holds mainscreen logic and drawing methods
class MainScreen {
    public MainScreen() {} 

    public void Draw(int score) {
        BeginDrawing();
        ClearBackground(BLACK);
        DrawText("GEM MANIA", 250, 50,  45, RED);
        DrawText($"prev score: {score}", 250, 100, 15, WHITE);
        DrawText("click anywhere to start", 250, 350, 15, WHITE);
        int high = 0;
        int result;
        int count = 0;
        foreach (string input in System.IO.File.ReadLines(@"HighScores.txt"))
        {  
            try
            {
                result = Int32.Parse(input);
                if (result > high) {
                high = result;
                } 
            }
            catch (FormatException)
            {
                Console.WriteLine($"Unable to parse '{input}'");
            }   
        }  
        foreach (string input in System.IO.File.ReadLines(@"HighScores.txt"))
        {  
            DrawText($"high score: {high}", 250, 225, 15, WHITE);
            if (count < 6) {
                DrawText($"{input}", 300, 125 + count * 15, 15, WHITE);   
            }
            count++;
        }  
        EndDrawing();
    }
}

// all collision methods
// seperated by object collision because they have different dimensions which make collisions a seperate deal
class Collision {

    public Collision() {}
    public bool CheckCollisionAsteroid(Player player, Asteroid asteroid) {
        if (player.x + player.width > asteroid.x && player.x < asteroid.x + asteroid.width && player.y < asteroid.y + asteroid.height && player.y + player.height > asteroid.y) {
            return true;
        }
        return false;
    }

    public bool CheckCollisionGem(Player player, Gem gem) {
        if (player.x + player.width > gem.x && player.x < gem.x + gem.width && player.y < gem.y + gem.height && player.y + player.height > gem.y) {
            return true;
        }
        return false;
    }
}

// health class controls health value and health drawing
class Health {
    public int health;

    public Health(int sethealth) {
        health = sethealth;
    }
    public void DrawHealth() {
        //DrawRectangle(15, 15, health * 6, 10, RED);
        for (int i = 0; i < health; i ++) {
            DrawText("<3", i * 20 + 5, 15, 15, RED);
        }
    }

    public void TakeDamage() { // hmm i wonder what this method does
        health -= 1;
    }
}

// class for our score which includes a multiplier which increases as we collect gems but drops back to 1 if we collide with an asteroid
// drawing method, setting our multiplier value and increasing score
class Score {
    public int score;
    public int multiplier;

    public Score(int scr) {
        score = scr;
    }

    public void DrawScore() {
        DrawText($"{score}", 760, 15, 15, WHITE);
        DrawText($"x{multiplier}", 760, 30, 15, WHITE);
    }

    public void Set(int setmultplier) {
        multiplier = setmultplier;
    }

    public void Increase(int setmultiplier) {
        score += 10 * setmultiplier;
        
        
    }
}

// our player class controls our playable hashtag, movement logic and pertinent attributes
class Player {
    public int x;
    public int y;
    public int width = 12;
    public int height = 15;

    public Player(int xLoc, int yLoc) 
    {
        x = xLoc;
        y = yLoc;

    }

    public void DrawPlayer() {
        DrawText("#", x, y, 20, RED);
        //DrawRectangle(x, y, width, height, RED);
    }

    public void MoveRight() {
        if (x < 800) {
            x += 2;
        }
        
    }

    public void MoveLeft() {
        if (x > 0) {
            x -= 2;
        }
        
    }
}

// falling object parent class for gems and asteroids
// fall being the big method that is shared between the two sub-classes
class FallingObject {
    public int x;
    public int y;

    public int speed;

    public void Fall(int spd) {
        speed = spd;
        y += speed;
        if (y > 450) {
            Spawn();
        }
    }

    public void Spawn() {
        Random rnd = new Random();
        x = rnd.Next(800);
        y = rnd.Next(400) - 400;
    }
}

// child class asteroid controls asteroid drawing and logic
// width and height attributes are important for collisions
class Asteroid : FallingObject {

    public int width = 10;
    public int height = 20;

    
    public Asteroid(int xLoc, int yLoc) {
        x = xLoc;
        y = yLoc;
    }

    public void DrawAsteroid() {
        DrawText("[]", x, y, 20, GREEN);
        //DrawRectangle(x, y, width, height, GREEN);
    }
}

// Gem class, child object of FallingObject class
// drawing the gem method and pertinent attributes found here!!!
class Gem : FallingObject {

    public int width = 10;
    public int height = 10;
    public Gem(int xLoc, int yLoc) {
        x = xLoc;
        y = yLoc;
    }

    public void DrawGem() {
        DrawText("*", x, y, 20, GREEN);
        //DrawRectangle(x, y, width, height, GREEN);
    }
}

// end of code
///         []  []                  []          *
///[]       []          []   [] * []
///     []                                  * []
/// []              []      []  *
///
///     []          *
///           []        []   []     []
///  []     * *     []
///                       #