package com.portfolio.portfolio.repository;

import com.portfolio.portfolio.model.Domain;
import org.springframework.data.mongodb.repository.MongoRepository;

public interface DomainRepository extends MongoRepository<Domain, String> {
    Domain findByUserId(String userId);
}
