namespace Elfencore.Shared.GameState
{
    public enum GamePhase
    {
        /*

        ~ Elvenland ~

        1. Deal travel cards (R1 != R1+) (Hadnled by GUI)
        2. Draw transportation counter -->              DrawCounterOnePhase
        3. Draw additional transportaion counter -->    DrawCounterTwoPhase && DrawCounterThreePhase
        4. Plan Travel Routes -->                       PlaceCounter
        5. Move the Elf Boots                           MoveBoot
        6. Finish round -->                             EndOfRound

        ~ Elvengold ~

        1. Draw travel cards (exep. R1) -->             DrawCardOnePhase/DrawCardTwoPhase/DrawCardThreePhase
        2. Distribute gold coins (exep. R1) (Handled by GUI)              
        3. Draw tokens and counters -->                 ChooseCounterPhase
        4. Auction -->                                  Auction
        5. Plan travel routes -->                       PlaceCounter
        6. Move elf boots -->                           MoveBoots
        7. Finish round  -->                            EndOfRound              

        */
        DrawCardOnePhase, DrawCardTwoPhase, DrawCardThreePhase,
        DrawCounterOnePhase, DrawCounterTwoPhase, DrawCounterThreePhase,
        PlaceCounter, MoveBoot, EndOfRound, Auction, ChooseCounterPhase, EndOfMoveBoot
    }
};