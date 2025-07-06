// Forgot Password & Reset Password JavaScript Functions

// OTP Timer functionality
let timeLeft = 600; // 10 minutes
let timerInterval;

// Initialize when document is ready
$(document).ready(function() {
    initializePage();
});

function initializePage() {
    // Check if we're on reset password page
    if ($('#resetForm').length > 0) {
        initializeResetPassword();
    }
    
    // Check if we're on forgot password page
    if ($('#forgotPasswordForm').length > 0) {
        initializeForgotPassword();
    }
}

function initializeForgotPassword() {
    // Form submission handling
    $('#forgotPasswordForm').on('submit', function(e) {
        if (!$(this).valid()) {
            return;
        }

        showLoadingState('#submitBtn');
        
        // Re-enable button on validation errors
        setTimeout(function() {
            if ($('.text-danger:visible, .alert-danger:visible').length > 0) {
                hideLoadingState('#submitBtn');
            }
        }, 100);
    });
    
    // Focus on email input
    $('#emailInput').focus();
}

function initializeResetPassword() {
    // Start countdown timer
    startTimer();
    
    // Initialize OTP inputs
    initializeOtpInputs();
    
    // Initialize password strength checker
    initializePasswordStrength();
    
    // Form submission handling
    $('#resetForm').on('submit', function(e) {
        updateOtpCode();
        
        // Validate OTP
        const otpCode = $('#otpCodeInput').val();
        if (otpCode.length !== 6) {
            e.preventDefault();
            showOtpError();
            return;
        }
        
        // Check form validity
        if (!$(this).valid()) {
            e.preventDefault();
            return;
        }

        showLoadingState('#resetBtn');
       
        // Re-enable button on validation errors
        setTimeout(function() {
            if ($('.text-danger:visible, .alert-danger:visible').length > 0) {
                hideLoadingState('#resetBtn');
            }
        }, 100);
    });
    
    // Focus on first OTP input
    $('#otp1').focus();
}

function initializeOtpInputs() {
    // OTP input handling with enhanced UX
    $('.otp-input').on('input', function() {
        // Only allow numbers
        this.value = this.value.replace(/[^0-9]/g, '');
        
        // Add filled class for visual feedback
        if (this.value.length === 1) {
            $(this).addClass('filled');
            const nextInput = $(this).next('.otp-input');
            if (nextInput.length) {
                nextInput.focus();
            }
        } else {
            $(this).removeClass('filled');
        }
        
        // Update hidden input
        updateOtpCode();
    });
    
    // Handle backspace and arrow navigation
    $('.otp-input').on('keydown', function(e) {
        if (e.key === 'Backspace') {
            if (this.value === '') {
                const prevInput = $(this).prev('.otp-input');
                if (prevInput.length) {
                    prevInput.focus();
                }
            } else {
                $(this).removeClass('filled');
            }
        } else if (e.key === 'ArrowLeft') {
            e.preventDefault();
            const prevInput = $(this).prev('.otp-input');
            if (prevInput.length) {
                prevInput.focus();
            }
        } else if (e.key === 'ArrowRight') {
            e.preventDefault();
            const nextInput = $(this).next('.otp-input');
            if (nextInput.length) {
                nextInput.focus();
            }
        }
    });
    
    // Handle paste events for OTP
    $('.otp-input').on('paste', function(e) {
        e.preventDefault();
        const pastedData = (e.originalEvent.clipboardData || window.clipboardData).getData('text');
        const numbers = pastedData.replace(/[^0-9]/g, '');
        
        if (numbers.length >= 6) {
            $('.otp-input').each(function(index) {
                if (index < 6 && index < numbers.length) {
                    $(this).val(numbers[index]).addClass('filled');
                }
            });
            updateOtpCode();
            $('#otp6').focus();
        }
    });
}

function initializePasswordStrength() {
    $('#newPasswordInput').on('input', function() {
        const password = $(this).val();
        const strengthDiv = $('#passwordStrength');
        
        if (password.length === 0) {
            strengthDiv.text('').removeClass('weak medium strong');
            return;
        }
        
        let strength = 0;
        let feedback = [];
        
        // Length checks
        if (password.length >= 6) strength++;
        if (password.length >= 8) strength++;
        
        // Character variety checks
        if (/[A-Z]/.test(password)) {
            strength++;
        } else {
            feedback.push('chữ hoa');
        }
        
        if (/[a-z]/.test(password)) {
            strength++;
        } else {
            feedback.push('chữ thường');
        }
        
        if (/[0-9]/.test(password)) {
            strength++;
        } else {
            feedback.push('số');
        }
        
        if (/[^A-Za-z0-9]/.test(password)) {
            strength++;
        } else {
            feedback.push('ký tự đặc biệt');
        }
        
        strengthDiv.removeClass('weak medium strong');
        
        if (strength <= 2) {
            strengthDiv.text('Mật khẩu yếu').addClass('weak');
        } else if (strength <= 4) {
            strengthDiv.text('Mật khẩu trung bình').addClass('medium');
        } else {
            strengthDiv.text('Mật khẩu mạnh').addClass('strong');
        }
    });
}

function updateOtpCode() {
    let otpCode = '';
    $('.otp-input').each(function() {
        otpCode += $(this).val();
    });
    $('#otpCodeInput').val(otpCode);
}

function showOtpError() {
    $('.otp-input').addClass('error');
    setTimeout(() => $('.otp-input').removeClass('error'), 500);
    alert('Vui lòng nhập đầy đủ 6 số OTP');
    $('#otp1').focus();
}

function startTimer() {
    const timer = $('#timer');
    const countdown = $('#countdown');
    
    timerInterval = setInterval(function() {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        
        timer.text(
            (minutes < 10 ? '0' : '') + minutes + ':' +
            (seconds < 10 ? '0' : '') + seconds
        );
        
        if (timeLeft <= 0) {
            clearInterval(timerInterval);
            countdown.addClass('expired');
            timer.text('Đã hết hạn');
        }
        
        timeLeft--;
    }, 1000);
}

function showLoadingState(buttonSelector) {
    const submitBtn = $(buttonSelector);
    const loginText = submitBtn.find('.login-text');
    const loadingSpinner = submitBtn.find('.loading-spinner');
    
    submitBtn.prop('disabled', true);
    loginText.hide();
    loadingSpinner.show();
}

function hideLoadingState(buttonSelector) {
    const submitBtn = $(buttonSelector);
    const loginText = submitBtn.find('.login-text');
    const loadingSpinner = submitBtn.find('.loading-spinner');
    
    submitBtn.prop('disabled', false);
    loginText.show();
    loadingSpinner.hide();
}

// Utility functions
function clearOtpInputs() {
    $('.otp-input').val('').removeClass('filled error');
    $('#otpCodeInput').val('');
}

function resetTimer() {
    timeLeft = 600;
    clearInterval(timerInterval);
    $('#countdown').removeClass('expired');
    startTimer();
} 