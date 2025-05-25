import { Component } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';

interface FaqItem {
    question: string;
    answer: string;
    category: 'general' | 'delivery' | 'payment' | 'robot';
}

@Component({
    selector: 'app-faq',
    standalone: true,
    imports: [NgFor, NgIf],
    templateUrl: './faq.component.html',
    styleUrl: './faq.component.css'
})
export class FaqComponent {
    selectedCategory: 'all' | 'general' | 'delivery' | 'payment' | 'robot' = 'all';
    expandedItems: Set<number> = new Set();

    faqItems: FaqItem[] = [
        {
            category: 'general',
            question: 'What is Digital Delivery?',
            answer: 'Digital Delivery is an innovative delivery service that uses autonomous robots to deliver packages. Our service combines cutting-edge technology with reliable delivery solutions to provide fast and efficient package delivery.'
        },
        {
            category: 'general',
            question: 'How do I create an account?',
            answer: 'You can create an account by clicking the "Sign Up" button on our homepage. You\'ll need to provide your email address, create a password, and fill in your personal information. After registration, you can start using our delivery services.'
        },
        {
            category: 'delivery',
            question: 'How do I track my delivery?',
            answer: 'You can track your delivery in real-time through your account dashboard. Each order has a unique tracking number, and you can see the robot\'s location and estimated delivery time. You\'ll also receive notifications about your delivery status.'
        },
        {
            category: 'delivery',
            question: 'What are the delivery hours?',
            answer: 'Our delivery service operates 24/7. However, delivery times may vary depending on your location and the current demand. You can select your preferred delivery time when placing an order.'
        },
        {
            category: 'delivery',
            question: 'How do the delivery robots work?',
            answer: 'Our delivery robots are autonomous vehicles equipped with advanced navigation systems. They can safely navigate through urban environments, avoid obstacles, and deliver packages to specified locations. Each robot is monitored by our control center for safety and efficiency.'
        },
        {
            category: 'payment',
            question: 'What payment methods do you accept?',
            answer: 'We accept various payment methods including credit/debit cards, PayPal, and other major digital payment systems. All payments are processed securely through our encrypted payment system.'
        },
        {
            category: 'payment',
            question: 'Is my payment information secure?',
            answer: 'Yes, we use industry-standard encryption to protect your payment information. All transactions are processed through secure payment gateways, and we never store your complete payment details on our servers.'
        },
        {
            category: 'robot',
            question: 'How do I interact with the delivery robot?',
            answer: 'When the robot arrives at your location, you\'ll receive a notification. You can unlock the delivery compartment using the code provided in the app. The robot will wait for you to collect your package before continuing its route.'
        },
        {
            category: 'robot',
            question: 'What happens if the robot encounters an issue?',
            answer: 'Our robots are equipped with multiple safety systems and are constantly monitored by our control center. If any issue occurs, our support team is immediately notified and can assist remotely or dispatch a technician if needed.'
        }
    ];

    get filteredFaqItems(): FaqItem[] {
        if (this.selectedCategory === 'all') {
            return this.faqItems;
        }
        return this.faqItems.filter(item => item.category === this.selectedCategory);
    }

    toggleItem(index: number): void {
        if (this.expandedItems.has(index)) {
            this.expandedItems.delete(index);
        } else {
            this.expandedItems.add(index);
        }
    }

    isExpanded(index: number): boolean {
        return this.expandedItems.has(index);
    }

    selectCategory(category: 'all' | 'general' | 'delivery' | 'payment' | 'robot'): void {
        this.selectedCategory = category;
        this.expandedItems.clear();
    }
} 