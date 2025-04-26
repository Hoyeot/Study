package com.portfolio.portfolio.dto;

public class RankUpdateRequest {
    private String userId;
    private String time;

    public RankUpdateRequest() {}

    public RankUpdateRequest(String userId, String time) {
        this.userId = userId;
        this.time = time;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public String getTime() {
        return time;
    }

    public void setTime(String time) {
        this.time = time;
    }
}