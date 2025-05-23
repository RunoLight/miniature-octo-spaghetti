﻿using System.Linq;
using Application.Services;
using Domain.Interfaces;

namespace Application.UseCases
{
    public class MatchCheckUseCase
    {
        private readonly IMatchCheckService _matchChecker;
        private readonly IGameStateService _gameStateService;
        private readonly ISoundPlayer _soundPlayer;
        private readonly SaveGameUseCase _saveGameUseCase;
        private readonly ScoreService _scoreService;

        private const int MatchPointsReward = 10;
        private const int FailMatchPointsCost = 1;

        public MatchCheckUseCase(
            IMatchCheckService matchChecker, IGameStateService gameStateService,
            ISoundPlayer soundPlayer, SaveGameUseCase saveGameUseCase,
            ScoreService scoreService
        )
        {
            _matchChecker = matchChecker;
            _gameStateService = gameStateService;
            _soundPlayer = soundPlayer;
            _saveGameUseCase = saveGameUseCase;
            _scoreService = scoreService;
        }

        public void Execute(int firstCardId, int secondCardId)
        {
            var matchResult = _matchChecker.TryMatch(_gameStateService.GetPairCards(firstCardId, secondCardId));
            if (matchResult.IsMatch)
            {
                _gameStateService.MatchCards(matchResult.Cards.Select(c => c.Id).ToList());
                _soundPlayer.Play(SoundType.Match);
            }
            else
            {
                _gameStateService.ResetUnmatchedCards();
                _soundPlayer.Play(SoundType.Mismatch);
            }

            _scoreService.AddPoints(matchResult.IsMatch ? MatchPointsReward : FailMatchPointsCost);
            _saveGameUseCase.Execute();
        }
    }
}