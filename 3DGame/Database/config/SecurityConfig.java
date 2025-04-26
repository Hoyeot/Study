package com.portfolio.portfolio.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.web.SecurityFilterChain;

@Configuration
public class SecurityConfig {
    @Bean
    public PasswordEncoder passwordEncoder() {
        return new BCryptPasswordEncoder();
     }
    @Bean
    public SecurityFilterChain filterChain(HttpSecurity http) throws Exception {
        http
            .csrf(csrf -> csrf.disable())  // CSRF 보호 해제
            .authorizeHttpRequests(auth -> auth
                .requestMatchers("/**").permitAll()  // 모든 경로 허용
                .anyRequest().permitAll()
            )
            .headers(headers -> headers
                .frameOptions().disable()  // H2 콘솔 등 iframe 허용
            );
        return http.build();
    }
}