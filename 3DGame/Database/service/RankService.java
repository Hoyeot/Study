package com.portfolio.portfolio.service;

import com.portfolio.portfolio.model.Rank;
import com.portfolio.portfolio.repository.RankRepository;
import org.springframework.stereotype.Service;
import java.util.List;

@Service
public class RankService {
    private final RankRepository rankRepository;

    public RankService(RankRepository rankRepository) {
        this.rankRepository = rankRepository;
    }

    public Rank saveRank(String userId, String time) {
        Rank rank = rankRepository.findByUserId(userId);
        if (rank == null) {
            rank = new Rank(userId, time);
        } else {
            rank.setTime(time);
        }
        return rankRepository.save(rank);
    }

    public List<Rank> getAllRanks() {
        return rankRepository.findAllByOrderByTimeAsc();
    }

    public Rank findByUserId(String userId) {
        return rankRepository.findByUserId(userId);
    }
}
