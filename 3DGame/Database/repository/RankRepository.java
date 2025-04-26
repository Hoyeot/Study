package com.portfolio.portfolio.repository;

import com.portfolio.portfolio.model.Rank;
import org.springframework.data.mongodb.repository.MongoRepository;
import java.util.List;

public interface RankRepository extends MongoRepository<Rank, String> {
    List<Rank> findAllByOrderByTimeAsc();
    Rank findByUserId(String userId);
}