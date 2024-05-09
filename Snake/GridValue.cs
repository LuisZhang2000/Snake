namespace Snake
{
    public enum GridValue
    {
        // position in the grid can either be empty, contain part of the snake or contain the food
        Empty, 
        Snake,
        Food,
        Outside // used for when snake tries to move outside the grid 
    }
}
