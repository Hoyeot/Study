package com.portfolio.portfolio.controller;

import com.portfolio.portfolio.model.Rank;
import com.portfolio.portfolio.service.RankService;
import com.portfolio.portfolio.dto.RankUpdateRequest;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.List;


@RestController
@RequestMapping("/api/rank")
public class RankController {
    private final RankService rankService;

    public RankController(RankService rankService) {
        this.rankService = rankService;
    }

    @GetMapping("/all")
    public ResponseEntity<List<Rank>> getAllRanks() {
        List<Rank> ranks = rankService.getAllRanks();
        return ResponseEntity.ok(ranks);
    }

    @GetMapping("/{userId}")
    public ResponseEntity<Rank> getMyRank(@PathVariable String userId) {
        Rank rank = rankService.findByUserId(userId);
        return rank != null ? ResponseEntity.ok(rank) : ResponseEntity.notFound().build();
    }

    @GetMapping("/search")
    public ResponseEntity<Rank> searchRank(
        @RequestParam String userId
    ) {
        Rank rank = rankService.findByUserId(userId);
        return rank != null 
            ? ResponseEntity.ok(rank)
            : ResponseEntity.notFound().build();
    }

    @PostMapping("/update")
    public ResponseEntity<String> updateOrInsertRank(@RequestBody RankUpdateRequest request) {
        try {
            rankService.saveRank(request.getUserId(), request.getTime());
            return ResponseEntity.ok("기록 저장 성공");
        } catch (Exception e) {
            return ResponseEntity.badRequest().body("오류: " + e.getMessage());
        }
    }

    @PostMapping("path")
    public ResponseEntity<Rank> saveRank(@RequestParam String userId, @RequestParam String time) {
        return ResponseEntity.ok(rankService.saveRank(userId, time));
    }
    
}