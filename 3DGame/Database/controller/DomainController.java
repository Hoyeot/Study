package com.portfolio.portfolio.controller;

import com.portfolio.portfolio.model.Domain;
import com.portfolio.portfolio.service.DomainService;

import org.springframework.http.ResponseEntity;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;


@RestController
@RequestMapping("/api/user")

public class DomainController {
    private final DomainService service;
    private final PasswordEncoder passwordEncoder;

    public DomainController(DomainService service, PasswordEncoder passwordEncoder) {
        this.service = service;
        this.passwordEncoder = passwordEncoder;
    }

    // 회원가입
    @PostMapping("/regist")
    public ResponseEntity<?> register(@RequestBody Domain user) {
        try {
            if (service.findByUserId(user.getUserId()) != null) {
                return ResponseEntity.badRequest().body("이미 존재하는 아이디입니다.");
            }
            Domain newUser = service.createUser(user.getUserId(), user.getPassword());
            return ResponseEntity.ok(newUser);
        } catch (Exception e) {
            return ResponseEntity.badRequest().body("회원가입 실패: " + e.getMessage());
        }
    }

    // 로그인
    @PostMapping("/login")
    public ResponseEntity<?> login(@RequestBody LoginRequest request) {
        Domain foundUser = service.findByUserId(request.userId());  // record는 getter 없이 필드명()으로 접근
        
        if (foundUser == null) {
            return ResponseEntity.status(401).body("아이디가 존재하지 않습니다");
        }

        if (passwordEncoder.matches(request.password(), foundUser.getPassword())) {
            return ResponseEntity.ok("로그인 성공");
        } else {
            return ResponseEntity.status(401).body("비밀번호가 일치하지 않습니다");
        }
    }
    
    @GetMapping("/{userId}")
    public Domain get(@PathVariable String userId) {
        return service.findByUserId(userId);
    }

    public record LoginRequest(String userId, String password) {}
}