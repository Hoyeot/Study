package com.portfolio.portfolio.model;

import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

@Document(collection = "rank")
public class Rank {
    @Id
    private String id;
    private String userId;
    private String time;

    public Rank() {}

    public Rank(String userId, String time) {
        this.userId = userId;
        this.time = time;
    }

    public String getId() { return id; }
    public void setId(String id) { this.id = id; }
    public String getUserId() { return userId; }
    public void setUserId(String userId) { this.userId = userId; }
    public String getTime() { return time; }
    public void setTime(String time) { this.time = time; }
}
