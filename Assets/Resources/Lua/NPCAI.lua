-- 和Player的Move逻辑一致，用velocity移动+固定Y轴
function UpdateNPCMovement(rb, moveDir, moveSpeed)
    -- 物理移动（和Player的_rb.velocity一致）
    local targetVelocity = moveDir * moveSpeed
    -- 固定Y轴（和Player一样，保持Y=0）
    targetVelocity.y = 0
    -- 赋值给Rigidbody的速度
    rb.velocity = targetVelocity
end