package com.portfolio.portfolio.service;

import com.portfolio.portfolio.model.Domain;
import com.portfolio.portfolio.model.Rank;
import com.portfolio.portfolio.repository.DomainRepository;
import com.portfolio.portfolio.repository.RankRepository;

import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

@Service
public class DomainService {
    private final DomainRepository repository;
    private final RankRepository rankRepository;
    private final PasswordEncoder passwordEncoder;

    public DomainService(DomainRepository repository, RankRepository rankRepository, PasswordEncoder passwordEncoder) {
        this.repository = repository;
        this.rankRepository = rankRepository;
        this.passwordEncoder = passwordEncoder;
    }

    public Domain createUser(String userId, String password) {
        if (repository.findByUserId(userId) != null) {
            throw new RuntimeException("이미 존재하는 아이디입니다.");
        }
        
        String encodedPassword = passwordEncoder.encode(password);
        Domain user = new Domain(userId, encodedPassword);
        return repository.save(user);
    }

    public boolean validateUser(String userId, String rawPassword) {
        Domain user = repository.findByUserId(userId);
        return user != null && passwordEncoder.matches(rawPassword, user.getPassword());
    }

    public Domain findByUserId(String userId) {
        return repository.findByUserId(userId);
    }

    public Rank getRankByUserId(String userId) {
        return rankRepository.findByUserId(userId);
    }
}