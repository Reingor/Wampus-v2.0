using System;
using System.Collections.Generic;
using System.Windows.Input;


interface ICommand
{
    void Execute();
}

// Конкретная команда для движения игрока
class MoveCommand : ICommand
{
    private Player player;
    private int newX, newY;

    public MoveCommand(Player player, int newX, int newY)
    {
        this.player = player;
        this.newX = newX;
        this.newY = newY;
    }

    public void Execute()
    {
        player.Move(newX, newY);
    }
}

// Конкретная команда для стрельбы
class ShootCommand : ICommand
{
    private Player player;
    private World world;
    private int directionX, directionY;

    public ShootCommand(Player player, World world, int directionX, int directionY)
    {
        this.player = player;
        this.world = world;
        this.directionX = directionX;
        this.directionY = directionY;
    }

    public void Execute()
    {
        player.ShootArrow(directionX, directionY, world);
    }
}

// Класс комнаты
class Room
{
    public char Content { get; set; }

    public Room(char content)
    {
        Content = content;
    }

    public void Replace(char f)
    {
        Content = f;
    }

    public bool IsEmpty()
    {
        return Content == '_';
    }

    public override string ToString()
    {
        return Content.ToString();
    }
}

// Класс игрока
class Player
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Player(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Move(int newX, int newY)
    {
        X = newX;
        Y = newY;
    }

    public void ShootArrow(int directionX, int directionY, World world)
    {
        int targetX = X;
        int targetY = Y;

        while (world.GetTile(targetX, targetY).Content != '_')
        {
            if (world.GetTile(targetX, targetY).Content == 'W')
            {
                Console.WriteLine("Congratulations! You shot the Wumpus and won the game.");
                Environment.Exit(0);
            }
            targetX += directionX;
            targetY += directionY;
        }

        Console.WriteLine("You missed.  The arrow hit a wall.");
    }

    public bool IsAdjacentToWumpus(Room[,] rooms)
    {
        return (Math.Abs(X - rooms.GetLength(0)) == 1 && Y == rooms.GetLength(1)) || (X == rooms.GetLength(0) && Math.Abs(Y - rooms.GetLength(1)) == 1);
    }

    public bool IsAdjacentToPit(Room[,] rooms)
    {
        int xLength = rooms.GetLength(0);
        int yLength = rooms.GetLength(1);

        for (int x = Math.Max(0, X - 1); x <= Math.Min(xLength - 1, X + 1); x++)
        {
            for (int y = Math.Max(0, Y - 1); y <= Math.Min(yLength - 1, Y + 1); y++)
            {
                if (rooms[x, y].Content == 'P')
                {
                    return true;
                }
            }
        }

        return false;
    }
}

// Класс мира
class World
{
    public Room[,] rooms;
    private Random random = new Random();

    public World(int size)
    {
        rooms = new Room[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                rooms[i, j] = new Room('_');
            }
        }
    }

    public Room GetTile(int x, int y)
    {
        if (x >= 0 && x < rooms.GetLength(0) && y >= 0 && y < rooms.GetLength(1))
        {
            return rooms[x, y];
        }
        return null;
    }

    public void SetTile(int x, int y, char tile)
    {
        if (x >= 0 && x < rooms.GetLength(0) && y >= 0 && y < rooms.GetLength(1))
        {
            rooms[x, y].Content = tile;
        }
    }

    public void GenerateWorld()
    {
        int treasureX = random.Next(rooms.GetLength(0));
        int treasureY = random.Next(rooms.GetLength(1));

        int pit1X = random.Next(rooms.GetLength(0));
        int pit1Y = random.Next(rooms.GetLength(1));

        int pit2X = random.Next(rooms.GetLength(0));
        int pit2Y = random.Next(rooms.GetLength(1));

        while ((treasureX == pit1X && treasureY == pit1Y) ||
               (treasureX == pit2X && treasureY == pit2Y) ||
               (pit1X == pit2X && pit1Y == pit2Y))
        {
            treasureX = random.Next(rooms.GetLength(0));
            treasureY = random.Next(rooms.GetLength(1));

            pit1X = random.Next(rooms.GetLength(0));
            pit1Y = random.Next(rooms.GetLength(1));

            pit2X = random.Next(rooms.GetLength(0));
            pit2Y = random.Next(rooms.GetLength(1));
        }

        SetTile(treasureX, treasureY, 'G');
        SetTile(pit1X, pit1Y, 'P');
        SetTile(pit2X, pit2Y, 'P');

        int wumpusX = random.Next(rooms.GetLength(0));
        int wumpusY = random.Next(rooms.GetLength(1));

        while ((wumpusX == treasureX && wumpusY == treasureY) ||
               (wumpusX == pit1X && wumpusY == pit1Y) ||
               (wumpusX == pit2X && wumpusY == pit2Y))
        {
            wumpusX = random.Next(rooms.GetLength(0));
            wumpusY = random.Next(rooms.GetLength(1));
        }

        SetTile(wumpusX, wumpusY, 'W');
    }

    public int GetLength(int dimension)
    {
        return rooms.GetLength(dimension);
    }
}

class WumpusGame
{
    private World world;
    private Player player;
    private int arrowsCount;
    private Random random = new Random();
    private List<ICommand> commandHistory = new List<ICommand>();

    public WumpusGame(int worldSize, int arrowsCount)
    {
        world = new World(worldSize);
        player = new Player(0, 0);
        this.arrowsCount = arrowsCount;
    }

    public void Start()
    {
        Console.WriteLine("Welcome to Wumpus World!");
        world.GenerateWorld();
        PrintWorld();
        CheckForWumpusSmell();
        CheckForPitDraft();

        while (true)
        {
            MoveWumpus();
            Console.Write("Enter your move (W/A/S/D) or 'F' to shoot: ");
            char move = Console.ReadKey().KeyChar;
            Console.WriteLine();

            if (move == 'F')
            {
                Console.Write("Enter the direction to shoot (W/A/S/D): ");
                char shootDirection = Console.ReadKey().KeyChar;
                int directionX = 0;
                int directionY = 0;

                switch (shootDirection)
                {
                    case 'W':
                        directionX = -1;
                        break;
                    case 'A':
                        directionY = -1;
                        break;
                    case 'S':
                        directionX = 1;
                        break;
                    case 'D':
                        directionY = 1;
                        break;
                    default:
                        Console.WriteLine("Invalid direction. Use W/A/S/D to shoot.");
                        continue;
                }

                ICommand shootCommand = new ShootCommand(player, world, directionX, directionY);
                ExecuteCommand(shootCommand);
                continue;
            }

            int newX = player.X;
            int newY = player.Y;

            switch (move)
            {
                case 'W':
                    newX--;
                    break;
                case 'A':
                    newY--;
                    break;
                case 'S':
                    newX++;
                    break;
                case 'D':
                    newY++;
                    break;
                default:
                    Console.WriteLine("Invalid move. Use W/A/S/D to move or 'F' to shoot.");
                    continue;
            }

            ICommand moveCommand = new MoveCommand(player, newX, newY);
            ExecuteCommand(moveCommand);
        }
    }

    private void PrintWorld()
    {
        Console.Clear();

        for (int i = 0; i < world.GetLength(0); i++)
        {
            for (int j = 0; j < world.GetLength(1); j++)
            {
                Room room = world.GetTile(i, j);
                if (i == player.X && j == player.Y)
                    Console.Write("P ");
                else
                    Console.Write(room + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Arrows: " + arrowsCount);
    }

    private void MoveWumpus()
    {
        if (random.NextDouble() < 0.75)
        {
            int newX, newY;

            do
            {
                int moveDirection = random.Next(4);

                switch (moveDirection)
                {
                    case 0:
                        newX = player.X - 1;
                        newY = player.Y;
                        break;
                    case 1:
                        newX = player.X + 1;
                        newY = player.Y;
                        break;
                    case 2:
                        newX = player.X;
                        newY = player.Y - 1;
                        break;
                    case 3:
                        newX = player.X;
                        newY = player.Y + 1;
                        break;
                    default:
                        newX = player.X;
                        newY = player.Y;
                        break;
                }
            } while (!IsValidTile(newX, newY));

            world.SetTile(player.X, player.Y, new Room('_').Content);
            world.SetTile(newX, newY, new Room('W').Content);
        }
    }

    private void CheckForWumpusSmell()
    {
        bool wumpusSmell = player.IsAdjacentToWumpus(world.rooms);
        if (wumpusSmell)
        {
            Console.WriteLine("I smell a Wumpus");
        }
    }

    private void CheckForPitDraft()
    {
        bool pitDraft = player.IsAdjacentToPit(world.rooms);
        if (pitDraft)
        {
            Console.WriteLine("I feel a draft");
        }
    }

    private void ExecuteCommand(ICommand command)
    {
        command.Execute();
        commandHistory.Add(command);
        PrintWorld();
        CheckForWumpusSmell();
        CheckForPitDraft();
    }

    private bool IsValidTile(int x, int y)
    {
        return x >= 0 && x < world.GetLength(0) && y >= 0 && y < world.GetLength(1);
    }
}

class Program
{
    static void Main(string[] args)
    {
        int worldSize = 4; // Размер мира. Можете изменить на нужное значение.
        int arrowsCount = 1; // Количество стрел. Можете изменить на нужное значение.

        WumpusGame game = new WumpusGame(worldSize, arrowsCount);
        game.Start();
    }
}
