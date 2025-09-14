// Language Switcher JavaScript
(function() {
    'use strict';

    // Language translations
    const translations = {
        bg: {
            'Home': 'Начало',
            'About': 'За нас',
            'Menu': 'Продукти',
            'Chefs': 'Пчелари',
            'Contact': 'Контакт',
            'Our Beekeepers': 'Нашите Пчелари',
            'Team Members': 'Екипът ни',
            'Beekeeper': 'Пчелар',
            'About Us': 'За нас',
            'My Account': 'Моят акаунт',
            'Shopping Cart': 'Моята поръчка',
            'Our Shop': 'Онлайн магазин',
            'Contact us': 'Свържете се с нас',
            'Newsletter': 'Новини',
            'Address': 'Адрес',
            'Phone': 'Телефон',
            'Email': 'Имейл',
            'Team Member': 'Екипът ни',
            'Chef Master': 'Пчелар'
        },
        en: {
            'Начало': 'Home',
            'За нас': 'About',
            'Продукти': 'Menu',
            'Пчелари': 'Chefs',
            'Контакт': 'Contact',
            'Нашите Пчелари': 'Our Beekeepers',
            'Екипът ни': 'Team Members',
            'Пчелар': 'Beekeeper',
            'Моят акаунт': 'My Account',
            'Моята поръчка': 'Shopping Cart',
            'Онлайн магазин': 'Our Shop',
            'Свържете се с нас': 'Contact us',
            'Новини': 'Newsletter',
            'Адрес': 'Address',
            'Телефон': 'Phone',
            'Имейл': 'Email',
            'Chef Master': 'Chef Master'
        }
    };

    // Create language switcher HTML
    function createLanguageSwitcher() {
        const switcher = document.createElement('div');
        switcher.className = 'language-switcher';
        switcher.innerHTML = `
            <div class="btn-group" role="group">
                <button type="button" id="lang-bg" class="btn btn-sm btn-primary active" onclick="switchLanguage('bg')">БГ</button>
                <button type="button" id="lang-en" class="btn btn-sm btn-outline-primary" onclick="switchLanguage('en')">EN</button>
            </div>
        `;
        
        // Add styles
        const style = document.createElement('style');
        style.textContent = `
            .language-switcher {
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 1000;
            }
            
            .language-switcher .btn-group .btn {
                border-radius: 0;
                font-weight: bold;
                font-size: 12px;
                padding: 5px 10px;
            }
            
            .language-switcher .btn-group .btn:first-child {
                border-top-left-radius: 4px;
                border-bottom-left-radius: 4px;
            }
            
            .language-switcher .btn-group .btn:last-child {
                border-top-right-radius: 4px;
                border-bottom-right-radius: 4px;
            }
            
            .language-switcher .btn-primary {
                background-color: #ff6b35;
                border-color: #ff6b35;
            }
            
            .language-switcher .btn-outline-primary {
                color: #ff6b35;
                border-color: #ff6b35;
            }
            
            .language-switcher .btn-outline-primary:hover {
                background-color: #ff6b35;
                border-color: #ff6b35;
                color: white;
            }
        `;
        
        document.head.appendChild(style);
        document.body.appendChild(switcher);
    }

    // Switch language function
    window.switchLanguage = function(lang) {
        // Update button states
        const bgBtn = document.getElementById('lang-bg');
        const enBtn = document.getElementById('lang-en');
        
        if (bgBtn && enBtn) {
            bgBtn.classList.toggle('active', lang === 'bg');
            bgBtn.classList.toggle('btn-primary', lang === 'bg');
            bgBtn.classList.toggle('btn-outline-primary', lang !== 'bg');
            
            enBtn.classList.toggle('active', lang === 'en');
            enBtn.classList.toggle('btn-primary', lang === 'en');
            enBtn.classList.toggle('btn-outline-primary', lang !== 'en');
        }

        // Translate all elements with data-translate attribute
        const elements = document.querySelectorAll('[data-translate]');
        elements.forEach(element => {
            const key = element.getAttribute('data-translate');
            if (translations[lang] && translations[lang][key]) {
                element.textContent = translations[lang][key];
            }
        });

        // Store language preference
        localStorage.setItem('language', lang);
    };

    // Load saved language preference
    function loadLanguage() {
        const savedLang = localStorage.getItem('language') || 'bg';
        switchLanguage(savedLang);
    }

    // Initialize when DOM is loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            createLanguageSwitcher();
            loadLanguage();
        });
    } else {
        createLanguageSwitcher();
        loadLanguage();
    }
})();
